using AutoFish.Utils;
using Terraria;
using Terraria.ID;
using TShockAPI;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AutoFish;

public partial class Plugin
{
    private void OnAI_061_FishingBobber(Projectile projectile,
        HookEvents.Terraria.Projectile.AI_061_FishingBobberEventArgs args)
    {
        this.HookUpdate(projectile);
        args.ContinueExecution = false;
    }
    private void HookUpdate(Projectile hook)
    {

        if (hook.ai[0] >= 1f)
        {
            return;
        }
        if (hook.owner < 0)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] hook.owner < 0");
            return;
        }

        if (hook.owner > Main.maxPlayers)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] hook.owner > Main.maxPlayers");
            return;
        }

        if (!hook.active)
        {
            
           DebugInfoLog($"[AutoFishR-DEBUG] hook is not active");
            return;
        }

        if (!hook.bobber)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] hook is not bobber");
            return;
        }

        if (!Configuration.Instance.Enabled)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] Plugin not enabled");
            return;
        }

        if (!Configuration.Instance.GlobalAutoFishFeatureEnabled)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] Global auto fish feature not enabled");
            return;
        }

        var player = TShock.Players[hook.owner];
        if (player == null)
        {
            DebugInfoLog($"[AutoFishR-DEBUG] Player is null (owner: {hook.owner})");
            return;
        }

        var blockMonsterCatch = Configuration.Instance.GlobalBlockMonsterCatch &&
                                HasFeaturePermission(player, "filter.monster");
        var skipFishingAnimation = Configuration.Instance.GlobalSkipFishingAnimation &&
                                   HasFeaturePermission(player, "skipanimation");
        var blockQuestFish = Configuration.Instance.GlobalBlockQuestFish &&
                             HasFeaturePermission(player, "filter.quest");
        var protectValuableBait = Configuration.Instance.GlobalProtectValuableBaitEnabled &&
                                  HasFeaturePermission(player, "bait.protect");

        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        if (!playerData.AutoFishEnabled)
        {
            if (!HasFeaturePermission(player, "fish"))
            {
                if (DebugMode)
                {
                    player.SendInfoMessage($"[DEBUG] No permission for fish feature");
                }
                return;
            }

            if (playerData.FirstFishHintShown)
            {
                if (DebugMode)
                {
                    player.SendInfoMessage($"[DEBUG] First fish hint already shown, auto fish not enabled");
                }
                return;
            }

            playerData.FirstFishHintShown = true;
            player.SendInfoMessage(GetString("Detected that you are fishing. Use /af fish to enable auto fishing."));
            return;
        }

        blockMonsterCatch &= playerData.BlockMonsterCatch;
        skipFishingAnimation &= playerData.SkipFishingAnimation;
        blockQuestFish &= playerData.BlockQuestFish;
        protectValuableBait &= playerData.ProtectValuableBaitEnabled;

        // Negative value means bite countdown, indicating a fish is hooked
        if (!(hook.ai[1] < 0))
        {
            return;
        }

        player.TPlayer.Fishing_GetBait(out var baitPower, out var baitType);
        if (baitType == 0)
        {
            player.SendErrorMessage(GetString("No bait left!"));
            player.SendInfoMessage(GetString("Auto fishing has stopped. Please add bait and recast."));
            ResetHook(hook);
            return;
        }

        // Protect valuable bait by moving it to the end of inventory
        if (protectValuableBait && Configuration.Instance.ValuableBaitItemIds.Contains(baitType))
        {
            if (TrySwapValuableBaitToBack(player, baitType, Configuration.Instance.ValuableBaitItemIds,
                    out var fromSlot, out var toSlot, out var fromType, out var toType))
            {
                player.SendData(PacketTypes.PlayerSlot, "", player.Index, fromSlot);
                player.SendData(PacketTypes.PlayerSlot, "", player.Index, toSlot);
                var fromName = TShock.Utils.GetItemById(fromType).Name;
                var toName = TShock.Utils.GetItemById(toType).Name;
                SendGradientMessage(player,
                    GetString("Valuable bait detected, swapped with last bait: {0} -> {1} (slot {2} ↔ {3})").SFormat(fromName, toName, fromSlot, toSlot));
                ResetHook(hook);
                return;
            }
            else
            {
                var baitName = TShock.Utils.GetItemById(baitType).Name;
                SendGradientMessage(player, GetString("Protected valuable bait [{0}], last one remaining, fishing stopped.").SFormat(baitName));
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        }

        // Enable auto fishing in normal and consumption modes
        if (Configuration.Instance.GlobalConsumptionModeEnabled)
        {
            if (!CanConsumeFish(player, playerData))
            {
                player.SendInfoMessage(GetString("Consumption mode enabled. You lack required items. Use /af list."));
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        }

        // Modify fishing results
        var noCatch = true;
        var activePlayerCount = TShock.Players.Count(p => p != null && p.Active && p.IsLoggedIn);
        var dropLimit = GetLimit(activePlayerCount);
        var catchMonster = false;

        for (var count = 0; noCatch && count < dropLimit; count++)
        {
            var context = Projectile._context;
            if (hook.TryBuildFishingContext(context))
            {
                if (blockQuestFish)
                {
                    context.Fisher.questFish = -1;
                }

                hook.SetFishingCheckResults(ref context.Fisher);
            }

            var catchId = hook.localAI[1];

            if (context.Fisher.rolledEnemySpawn > 0)
            {
                if (blockMonsterCatch)
                {
                    continue;
                }

                catchMonster = true;
                noCatch = false;
            }

            if (context.Fisher.rolledItemDrop > 0)
            {
                noCatch = false;
            }

            if (noCatch)
            {
                continue;
            }

            hook.localAI[1] = catchId;
            break;
        }

        if (noCatch)
        {
            if (DebugMode)
            {
                player.SendInfoMessage($"[DEBUG] No catch after {dropLimit} attempts");
            }

            ResetHook(hook);
            return;
        }

        // Set to reel state
        hook.ai[0] = 1.0f;

        var nowBaitType = (int) hook.localAI[2];

        var locate = LocateBait(player, nowBaitType);
        var pull = player.TPlayer.ItemCheck_CheckFishingBobber_ConsumeBait(hook,
            out var baitUsed);

        if (nowBaitType != baitUsed)
        {
            if (DebugMode) player.SendInfoMessage($"[DEBUG] Bait mismatch: now={nowBaitType}, used={baitUsed}");
            player.SendMessage(GetString("Bait mismatch"), Colors.CurrentLiquidColor);
            return;
        }

        if (!pull)
        {
            if (DebugMode) player.SendInfoMessage($"[DEBUG] Cannot pull, bait may be depleted");
            return;
        }

        if (playerData.BuffEnabled)
        {
            BuffUpdate(player);
        }

        player.TPlayer.ItemCheck_CheckFishingBobber_PullBobber(hook, baitUsed);
        player.SendData(PacketTypes.PlayerSlot, "", player.Index, locate);

        var origPos = hook.position;
        if (!catchMonster)
        {
            SpawnHook(player, hook, origPos);
            this.AddMultiHook(player, hook, origPos);
        }

        player.SendData(PacketTypes.ProjectileNew, "", hook.whoAmI);

        if (skipFishingAnimation)
        {
            player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
        }
    }

    private static void SpawnHook(TSPlayer player, Projectile hook, Vector2 pos, string uuid = "")
    {
        var velocity = new Vector2(0, 0);
        var index = SpawnProjectile.NewProjectile(
            hook.GetProjectileSource_FromThis(),
            pos, velocity, hook.type, 0, 0,
            player.Index, 0, 0, 0, -1, uuid);
        player.SendData(PacketTypes.ProjectileNew, "", index);
    }

    private static void ResetHook(Projectile projectile)
    {
        projectile.ai[1] = 0;
        projectile.localAI[1] = 0;
        projectile.localAI[1] += 240f;
    }

    private static int LocateBait(TSPlayer player, int baitUsed)
    {
        for (var i = 0; i < player.TPlayer.inventory.Length; i++)
        {
            var inventorySlot = player.TPlayer.inventory[i];
            if (inventorySlot.bait <= 0 || baitUsed != inventorySlot.type)
            {
                continue;
            }
            return i;
        }
        return 0;
    }

    private static void BuffUpdate(TSPlayer player)
    {
        if (!Configuration.Instance.GlobalBuffFeatureEnabled)
        {
            return;
        }

        foreach (var buff in Configuration.Instance.BuffDurations)
        {
            player.SetBuff(buff.Key, buff.Value);
        }
    }
}