using Terraria;
using Terraria.ID;
using TShockAPI;
using Microsoft.Xna.Framework;
using static AutoFish.Utils.Utils;
using HookProjectile = HookEvents.Terraria.Projectile;

namespace AutoFish;

public partial class Plugin
{
    private void OnAI_061_FishingBobber(Projectile projectile,
        HookProjectile.AI_061_FishingBobberEventArgs args)
    {
        HookUpdate(projectile);
        args.ContinueExecution = false;
    }

    private void HookUpdate(Projectile hook)
    {
        // Basic validation checks
        if (hook.ai[0] >= 1f) return;
        if (hook.owner < 0 || hook.owner > Terraria.Main.maxPlayers) return;
        if (!hook.active || !hook.bobber) return;

        // Plugin enabled checks
        if (!Configuration.Instance.Enabled || !Configuration.Instance.GlobalAutoFishFeatureEnabled)
            return;

        var player = TShock.Players[hook.owner];
        if (player == null) return;

        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);

        // Check if auto fishing is enabled for player
        if (!playerData.AutoFishEnabled)
        {
            if (!HasFeaturePermission(player, "fish")) return;

            if (!playerData.FirstFishHintShown)
            {
                playerData.FirstFishHintShown = true;
                player.SendInfoMessage("Detected fishing. Use /af fish to enable auto fishing.");
            }
            return;
        }

        // Negative ai[1] means a fish is biting
        if (!(hook.ai[1] < 0)) return;

        // Check for bait
        player.TPlayer.Fishing_GetBait(out var baitPower, out var baitType);
        if (baitType == 0)
        {
            player.SendErrorMessage("No bait left!");
            player.SendInfoMessage("Auto fishing has stopped. Please add bait and recast.");
            ResetHook(hook);
            return;
        }

        // Determine feature flags
        var blockMonsterCatch = Configuration.Instance.GlobalBlockMonsterCatch &&
                                HasFeaturePermission(player, "filter.monster") &&
                                playerData.BlockMonsterCatch;
        var skipFishingAnimation = Configuration.Instance.GlobalSkipFishingAnimation &&
                                   HasFeaturePermission(player, "skipanimation") &&
                                   playerData.SkipFishingAnimation;
        var blockQuestFish = Configuration.Instance.GlobalBlockQuestFish &&
                             HasFeaturePermission(player, "filter.quest") &&
                             playerData.BlockQuestFish;
        var protectValuableBait = Configuration.Instance.GlobalProtectValuableBaitEnabled &&
                                  HasFeaturePermission(player, "bait.protect") &&
                                  playerData.ProtectValuableBaitEnabled;

        // Protect valuable bait by swapping
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
                    $"Valuable bait detected, swapped with last bait: {fromName} -> {toName} (slot {fromSlot} ↔ {toSlot})");
                ResetHook(hook);
                return;
            }
            else
            {
                var baitName = TShock.Utils.GetItemById(baitType).Name;
                SendGradientMessage(player, $"Protected valuable bait [{baitName}], last one remaining, fishing stopped.");
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        }

        // Consumption mode check
        if (Configuration.Instance.GlobalConsumptionModeEnabled)
        {
            if (!CanConsumeFish(player, playerData))
            {
                player.SendInfoMessage("Consumption mode enabled. You lack required items. Use /af list.");
                ResetHook(hook);
                player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
                return;
            }
        }

        // Fishing logic - attempt to catch fish
        var noCatch = true;
        var catchMonster = false;
        var activePlayerCount = TShock.Players.Count(p => p?.Active == true && p.IsLoggedIn);
        var dropLimit = GetLimit(activePlayerCount);

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
                    continue;

                catchMonster = true;
                noCatch = false;
            }

            if (context.Fisher.rolledItemDrop > 0)
            {
                noCatch = false;
            }

            if (!noCatch)
            {
                hook.localAI[1] = catchId;
            }
        }

        if (noCatch)
        {
            DebugInfoLog($"[AutoFish-DEBUG] No catch after {dropLimit} attempts");
            ResetHook(hook);
            return;
        }

        // Set to reel state
        hook.ai[0] = 1.0f;

        var nowBaitType = (int)hook.localAI[2];
        var locate = LocateBait(player, nowBaitType);
        
        var pull = player.TPlayer.ItemCheck_CheckFishingBobber_ConsumeBait(hook, out var baitUsed);

        if (nowBaitType != baitUsed)
        {
            DebugInfoLog($"[AutoFish-DEBUG] Bait mismatch: now={nowBaitType}, used={baitUsed}");
            player.SendMessage("Bait mismatch", Colors.CurrentLiquidColor);
            return;
        }

        if (!pull)
        {
            DebugInfoLog($"[AutoFish-DEBUG] Cannot pull, bait may be depleted");
            return;
        }

        // Apply buffs if enabled
        if (playerData.BuffEnabled)
        {
            BuffUpdate(player);
        }

        // Pull the bobber and sync
        player.TPlayer.ItemCheck_CheckFishingBobber_PullBobber(hook, baitUsed);
        player.SendData(PacketTypes.PlayerSlot, "", player.Index, locate);

        var origPos = hook.position;
        
        // Only spawn new hook if not catching monster
        if (!catchMonster)
        {
            SpawnHook(player, hook, origPos);
            AddMultiHook(player, hook, origPos);
        }

        player.SendData(PacketTypes.ProjectileNew, "", hook.whoAmI);

        // Skip animation if enabled
        if (skipFishingAnimation)
        {
            player.SendData(PacketTypes.ProjectileDestroy, "", hook.whoAmI);
        }
    }

    private static void SpawnHook(TSPlayer player, Projectile hook, Vector2 pos, string uuid = "")
    {
        var velocity = Vector2.Zero;
        var index = Utils.SpawnProjectile.NewProjectile(
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
            if (inventorySlot.bait > 0 && baitUsed == inventorySlot.type)
            {
                return i;
            }
        }
        return 0;
    }

    private static void BuffUpdate(TSPlayer player)
    {
        if (!Configuration.Instance.GlobalBuffFeatureEnabled)
            return;

        foreach (var buff in Configuration.Instance.BuffDurations)
        {
            player.SetBuff(buff.Key, buff.Value);
        }
    }
}