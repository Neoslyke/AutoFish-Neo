using LazyAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Projectile = HookEvents.Terraria.Projectile;

namespace AutoFish;

[ApiVersion(2, 1)]
public partial class Plugin(Main game) : LazyPlugin(game)
{
    public const string AdminPermission = $"autofish.admin";
    public const string CommonPermission = $"autofish.common";
    public const string DenyPermissionPrefix = $"autofish.no.";

    internal static bool DebugMode = false;

    internal static AFPlayerData PlayerData = new();

    public override string Name => "AutoFish";

    public override string Author => "Neoslyke, ksqeib, 羽学, 少司命";

    public override Version Version => new(2, 1, 0);

    public override string Description => "Automatic fishing.";

    internal static AFPlayerData.ItemData CreateDefaultPlayerData(string playerName)
    {
        // Attempt to resolve current player to seed defaults from permissions
        var player = TShock.Players.FirstOrDefault(p => p != null && p.Active &&
                                                        p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        var canBuff = HasFeaturePermission(player, "buff");
        var canMulti = HasFeaturePermission(player, "multihook");
        var canFish = HasFeaturePermission(player, "fish");
        var canBlockMonster = HasFeaturePermission(player, "filter.monster");
        var canSkipAnimation = HasFeaturePermission(player, "skipanimation");
        var canBlockQuestFish = HasFeaturePermission(player, "filter.quest");
        var canProtectBait = HasFeaturePermission(player, "bait.protect");

        var defaultAutoFish = Configuration.Instance.DefaultAutoFishEnabled && canFish;
        var defaultBuff = Configuration.Instance.DefaultBuffEnabled && canBuff;
        var defaultMulti = Configuration.Instance.DefaultMultiHookEnabled && canMulti;
        var defaultBlockMonster = Configuration.Instance.GlobalBlockMonsterCatch && Configuration.Instance.DefaultBlockMonsterCatch &&
                                  canBlockMonster;
        var defaultSkipAnimation = Configuration.Instance.GlobalSkipFishingAnimation && Configuration.Instance.DefaultSkipFishingAnimation &&
                                   canSkipAnimation;
        var defaultBlockQuestFish = Configuration.Instance.GlobalBlockQuestFish && Configuration.Instance.DefaultBlockQuestFish &&
                                    canBlockQuestFish;
        var defaultProtectBait = Configuration.Instance.GlobalProtectValuableBaitEnabled && Configuration.Instance.DefaultProtectValuableBaitEnabled &&
                                 canProtectBait;

        return new AFPlayerData.ItemData
        {
            Name = playerName,
            AutoFishEnabled = defaultAutoFish,
            BuffEnabled = defaultBuff,
            HookMaxNum = Configuration.Instance.GlobalMultiHookMaxNum,
            MultiHookEnabled = defaultMulti,
            BlockMonsterCatch = defaultBlockMonster,
            SkipFishingAnimation = defaultSkipAnimation,
            BlockQuestFish = defaultBlockQuestFish,
            ProtectValuableBaitEnabled = defaultProtectBait,
            FirstFishHintShown = false
        };
    }

    internal static bool HasFeaturePermission(TSPlayer? player, string featureKey, bool allowCommon = true)
    {
        if (player == null) return false;

        if (player.HasPermission(AdminPermission)) return true;

        var denyPermission = $"{DenyPermissionPrefix}{featureKey}";
        if (player.HasPermission(denyPermission)) return false;

        if (allowCommon && player.HasPermission(CommonPermission)) return true;

        var allowPermission = $"autofish.{featureKey}";
        return player.HasPermission(allowPermission);
    }

    public override void Initialize()
    {
        ServerApi.Hooks.GamePostInitialize.Register(this, this.OnGamePostInitialize);
        Projectile.AI_061_FishingBobber += this.OnAI_061_FishingBobber;
    }

    private void OnGamePostInitialize(EventArgs args)
    {
        if (!Main.ServerSideCharacter)
        {
            TShock.Log.ConsoleError("========================================");
            TShock.Log.ConsoleError("[AutoFishR] CRITICAL ERROR: SSC is not enabled!");
            TShock.Log.ConsoleError("[AutoFishR] This plugin requires ServerSideCharacter to function properly.");
            TShock.Log.ConsoleError("[AutoFishR] Please enable SSC in tshock/sscconfig.json.");
            TShock.Log.ConsoleError("[AutoFishR] If you share this error screenshot, please kindly inform the person to enable SSC.");
            TShock.Log.ConsoleError("[AutoFishR] Plugin has been automatically disabled.");
            TShock.Log.ConsoleError("========================================");
            return;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.GamePostInitialize.Deregister(this, this.OnGamePostInitialize);
            Projectile.AI_061_FishingBobber -= this.OnAI_061_FishingBobber;
        }

        base.Dispose(disposing);
    }

}