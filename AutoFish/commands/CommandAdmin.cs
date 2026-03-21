using LazyAPI.Attributes;
using TShockAPI;

namespace AutoFish;

[Command("afa")]
[Permissions("autofish.admin")]
internal static class CommandAdmin
{
    #region toggle
    [Alias("fish")]
    public static void ToggleAutoFish(CommandArgs args)
    {
        Configuration.Instance.GlobalAutoFishFeatureEnabled = !Configuration.Instance.GlobalAutoFishFeatureEnabled;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalAutoFishFeatureEnabled ? "enabled" : "disabled")}] auto fishing."));
        Configuration.Instance.SaveTo();
    }

    [Alias("buff")]
    public static void ToggleAutoFishBuff(CommandArgs args)
    {
        Configuration.Instance.GlobalBuffFeatureEnabled = !Configuration.Instance.GlobalBuffFeatureEnabled;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalBuffFeatureEnabled ? "enabled" : "disabled")}] auto fishing buff."));
        Configuration.Instance.SaveTo();
    }

    [Alias("multi")]
    public static void ToggleAutoFishMulti(CommandArgs args)
    {
        Configuration.Instance.GlobalMultiHookFeatureEnabled = !Configuration.Instance.GlobalMultiHookFeatureEnabled;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalMultiHookFeatureEnabled ? "enabled" : "disabled")}] multi-hook feature."));
        Configuration.Instance.SaveTo();
    }

    [Alias("mod")]
    public static void ToggleAutoFishMod(CommandArgs args)
    {
        Configuration.Instance.GlobalConsumptionModeEnabled = !Configuration.Instance.GlobalConsumptionModeEnabled;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalConsumptionModeEnabled ? "enabled" : "disabled")}] consumption mode."));
        Configuration.Instance.SaveTo();
    }

    [Alias("monster")]
    public static void ToggleAutoFishMonster(CommandArgs args)
    {
        Configuration.Instance.GlobalBlockMonsterCatch = !Configuration.Instance.GlobalBlockMonsterCatch;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalBlockMonsterCatch ? "enabled" : "disabled")}] no monster catching."));
        Configuration.Instance.SaveTo();
    }

    [Alias("anim")]
    public static void ToggleAutoFishAnim(CommandArgs args)
    {
        Configuration.Instance.GlobalSkipFishingAnimation = !Configuration.Instance.GlobalSkipFishingAnimation;
        args.Player.SendSuccessMessage(GetString($"Player [{args.Player.Name}] has [c/92C5EC:{(Configuration.Instance.GlobalSkipFishingAnimation ? "enabled" : "disabled")}] skip fishing animation."));
        Configuration.Instance.SaveTo();
    }

    [Alias("debug")]
    public static void ToggleAutoFishDebug(CommandArgs args)
    {
        Plugin.DebugMode = !Plugin.DebugMode;
        args.Player.SendSuccessMessage(GetString($"Debug mode [c/92C5EC:{(Plugin.DebugMode ? "enabled" : "disabled")}]."));
        Configuration.Instance.SaveTo();
    }
    #endregion

    [Alias("del")]
    public static void DelBait(CommandArgs args, string type)
    {
        var selectItem = SelectItem(args.Player, type);
        if (selectItem == null)
            return;

        var msg = Configuration.Instance.BaitRewards.Remove(selectItem.type)
            ? GetString($"Successfully removed item from bait list: [i:{selectItem.type}]!")
            : GetString("Item is not in the bait list!");

        args.Player.SendInfoMessage(msg);
        Configuration.Instance.SaveTo();
    }

    [Alias("duo")]
    public static void SetMultiHookMax(CommandArgs args, uint max)
    {
        Configuration.Instance.GlobalMultiHookMaxNum = (int)max;
        Configuration.Instance.SaveTo();
        args.Player.SendSuccessMessage(GetString($"Multi-hook limit set to: [c/92C5EC:{max}] hooks."));
    }

    [Alias("add", "set")]
    public static void AddFishItem(CommandArgs args, string type, uint count, uint minutes)
    {
        var selectItem = SelectItem(args.Player, type);
        if (selectItem == null)
            return;

        Configuration.Instance.BaitRewards[selectItem.type] = new Configuration.BaitReward
        {
            Count = (int)count,
            Minutes = (int)minutes
        };

        Configuration.Instance.SaveTo();
        args.Player.SendSuccessMessage(GetString($"Set bait [i:{selectItem.type}] rule: every {count} → {minutes} minutes"));
    }

    [Alias("time")]
    public static void Time(CommandArgs args, string type, uint minutes)
    {
        var selectItem = SelectItem(args.Player, type);
        if (selectItem == null)
            return;

        var bait = Configuration.Instance.BaitRewards[selectItem.type];
        bait.Minutes = (int)minutes;

        Configuration.Instance.SaveTo();
        args.Player.SendSuccessMessage(GetString($"Updated bait [i:{selectItem.type}] rule: every {bait.Count} → {minutes} minutes"));
    }

    public static void Help(CommandArgs args)
    {
        args.Player.SendSuccessMessage(GetString("[Auto Fishing - Admin Commands]"));
        args.Player.SendSuccessMessage(GetString("Use /afa to manage auto fishing."));
        args.Player.SendSuccessMessage(GetString("/afa buff -- toggle global buff"));
        args.Player.SendSuccessMessage(GetString("/afa multi -- toggle multi-hook mode"));
        args.Player.SendSuccessMessage(GetString("/afa duo <num> -- set hook limit"));
        args.Player.SendSuccessMessage(GetString("/afa mod -- toggle consumption mode"));
        args.Player.SendSuccessMessage(GetString("/afa set <item> <count> <minutes> -- set bait rule"));
        args.Player.SendSuccessMessage(GetString("/afa time <item> <minutes> -- change duration"));
        args.Player.SendSuccessMessage(GetString("/afa add <item> <count> <minutes> -- add bait"));
        args.Player.SendSuccessMessage(GetString("/afa del <item> -- remove bait"));
        args.Player.SendSuccessMessage(GetString("/afa monster -- toggle monster catching"));
        args.Player.SendSuccessMessage(GetString("/afa anim -- toggle skip animation"));
    }

    private static Terraria.Item? SelectItem(TSPlayer player, string type)
    {
        var items = TShock.Utils.GetItemByIdOrName(type);

        if (items.Count > 1)
        {
            player.SendMultipleMatchError(items.Select(i => i.Name));
            return null;
        }

        if (items.Count == 0)
        {
            player.SendErrorMessage(GetString("Item does not exist!"));
            return null;
        }

        return items[0];
    }
}