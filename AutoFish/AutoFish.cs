using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using Terraria;
using HookProjectile = HookEvents.Terraria.Projectile;

namespace AutoFish;

[ApiVersion(2, 1)]
public partial class Plugin : TerrariaPlugin
{
    public const string AdminPermission = "autofish.admin";
    public const string CommonPermission = "autofish.common";
    public const string DenyPermissionPrefix = "autofish.no.";

    internal static bool DebugMode;
    internal static AFPlayerData PlayerData = new();

    public override string Name => "AutoFish";
    public override string Author => "Neoslyke, ksqeib, 羽学, 少司命";
    public override Version Version => new(2, 1, 0);
    public override string Description => "Automatic fishing.";

    public Plugin(Terraria.Main game) : base(game) { }

    public override void Initialize()
    {
        Configuration.Load();
        
        ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInitialize);
        HookProjectile.AI_061_FishingBobber += OnAI_061_FishingBobber;
        GeneralHooks.ReloadEvent += OnReload;
        
        RegisterCommands();
    }

    private void RegisterCommands()
    {
        Commands.ChatCommands.Add(new Command(AdminPermission, CommandAdmin.HandleCommand, "afa")
        {
            HelpText = "Auto fishing admin commands"
        });

        Commands.ChatCommands.Add(new Command("autofish", CommandPlayer.HandleCommand, "af")
        {
            HelpText = "Auto fishing player commands"
        });
    }

    private static void OnReload(ReloadEventArgs args)
    {
        Configuration.Load();
        args.Player.SendSuccessMessage("[AutoFishing] Config reloaded successfully");
    }

    internal static AFPlayerData.ItemData CreateDefaultPlayerData(string playerName)
    {
        var player = TShock.Players.FirstOrDefault(p => p?.Active == true &&
            p.Name.Equals(playerName, StringComparison.OrdinalIgnoreCase));

        return new AFPlayerData.ItemData
        {
            Name = playerName,
            AutoFishEnabled = Configuration.Instance.DefaultAutoFishEnabled && HasFeaturePermission(player, "fish"),
            BuffEnabled = Configuration.Instance.DefaultBuffEnabled && HasFeaturePermission(player, "buff"),
            HookMaxNum = Configuration.Instance.GlobalMultiHookMaxNum,
            MultiHookEnabled = Configuration.Instance.DefaultMultiHookEnabled && HasFeaturePermission(player, "multihook"),
            BlockMonsterCatch = Configuration.Instance.GlobalBlockMonsterCatch && 
                               Configuration.Instance.DefaultBlockMonsterCatch && 
                               HasFeaturePermission(player, "filter.monster"),
            SkipFishingAnimation = Configuration.Instance.GlobalSkipFishingAnimation && 
                                   Configuration.Instance.DefaultSkipFishingAnimation && 
                                   HasFeaturePermission(player, "skipanimation"),
            BlockQuestFish = Configuration.Instance.GlobalBlockQuestFish && 
                            Configuration.Instance.DefaultBlockQuestFish && 
                            HasFeaturePermission(player, "filter.quest"),
            ProtectValuableBaitEnabled = Configuration.Instance.GlobalProtectValuableBaitEnabled && 
                                         Configuration.Instance.DefaultProtectValuableBaitEnabled && 
                                         HasFeaturePermission(player, "bait.protect"),
            FirstFishHintShown = false
        };
    }

    internal static bool HasFeaturePermission(TSPlayer? player, string featureKey, bool allowCommon = true)
    {
        if (player == null) return false;
        if (player.HasPermission(AdminPermission)) return true;
        if (player.HasPermission($"{DenyPermissionPrefix}{featureKey}")) return false;
        if (allowCommon && player.HasPermission(CommonPermission)) return true;
        return player.HasPermission($"autofish.{featureKey}");
    }

    private void OnGamePostInitialize(EventArgs args)
    {
        if (!Terraria.Main.ServerSideCharacter)
        {
            TShock.Log.ConsoleError("========================================");
            TShock.Log.ConsoleError("[AutoFish] CRITICAL: SSC is not enabled!");
            TShock.Log.ConsoleError("[AutoFish] Enable SSC in tshock/sscconfig.json");
            TShock.Log.ConsoleError("========================================");
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.GamePostInitialize.Deregister(this, OnGamePostInitialize);
            HookProjectile.AI_061_FishingBobber -= OnAI_061_FishingBobber;
            GeneralHooks.ReloadEvent -= OnReload;
            Commands.ChatCommands.RemoveAll(c => c.Names.Contains("af") || c.Names.Contains("afa"));
        }
        base.Dispose(disposing);
    }
}