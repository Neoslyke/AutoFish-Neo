using TShockAPI;

namespace AutoFish;

internal static class CommandAdmin
{
    public static void HandleCommand(CommandArgs args)
    {
        var subcmd = args.Parameters.Count > 0 ? args.Parameters[0].ToLower() : "help";

        switch (subcmd)
        {
            case "fish":
                ToggleAutoFish(args);
                break;
            case "buff":
                ToggleAutoFishBuff(args);
                break;
            case "multi":
                ToggleAutoFishMulti(args);
                break;
            case "mod":
                ToggleAutoFishMod(args);
                break;
            case "monster":
                ToggleAutoFishMonster(args);
                break;
            case "anim":
                ToggleAutoFishAnim(args);
                break;
            case "debug":
                ToggleAutoFishDebug(args);
                break;
            case "del":
                DelBait(args);
                break;
            case "duo":
                SetMultiHookMax(args);
                break;
            case "add":
            case "set":
                AddFishItem(args);
                break;
            case "time":
                SetTime(args);
                break;
            default:
                ShowHelp(args);
                break;
        }
    }

    private static void ToggleAutoFish(CommandArgs args)
    {
        Configuration.Instance.GlobalAutoFishFeatureEnabled = !Configuration.Instance.GlobalAutoFishFeatureEnabled;
        var status = Configuration.Instance.GlobalAutoFishFeatureEnabled ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] auto fishing.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishBuff(CommandArgs args)
    {
        Configuration.Instance.GlobalBuffFeatureEnabled = !Configuration.Instance.GlobalBuffFeatureEnabled;
        var status = Configuration.Instance.GlobalBuffFeatureEnabled ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] auto fishing buff.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishMulti(CommandArgs args)
    {
        Configuration.Instance.GlobalMultiHookFeatureEnabled = !Configuration.Instance.GlobalMultiHookFeatureEnabled;
        var status = Configuration.Instance.GlobalMultiHookFeatureEnabled ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] multi-hook feature.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishMod(CommandArgs args)
    {
        Configuration.Instance.GlobalConsumptionModeEnabled = !Configuration.Instance.GlobalConsumptionModeEnabled;
        var status = Configuration.Instance.GlobalConsumptionModeEnabled ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] consumption mode.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishMonster(CommandArgs args)
    {
        Configuration.Instance.GlobalBlockMonsterCatch = !Configuration.Instance.GlobalBlockMonsterCatch;
        var status = Configuration.Instance.GlobalBlockMonsterCatch ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] no monster catching.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishAnim(CommandArgs args)
    {
        Configuration.Instance.GlobalSkipFishingAnimation = !Configuration.Instance.GlobalSkipFishingAnimation;
        var status = Configuration.Instance.GlobalSkipFishingAnimation ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"[{args.Player.Name}] has [c/92C5EC:{status}] skip fishing animation.");
        Configuration.Instance.Save();
    }

    private static void ToggleAutoFishDebug(CommandArgs args)
    {
        Plugin.DebugMode = !Plugin.DebugMode;
        var status = Plugin.DebugMode ? "enabled" : "disabled";
        args.Player.SendSuccessMessage($"Debug mode [c/92C5EC:{status}].");
    }

    private static void DelBait(CommandArgs args)
    {
        if (args.Parameters.Count < 2)
        {
            args.Player.SendErrorMessage("Usage: /afa del <item>");
            return;
        }

        var item = SelectItem(args.Player, args.Parameters[1]);
        if (item == null) return;

        var msg = Configuration.Instance.BaitRewards.Remove(item.type)
            ? $"Successfully removed item from bait list: [i:{item.type}]!"
            : "Item is not in the bait list!";

        args.Player.SendInfoMessage(msg);
        Configuration.Instance.Save();
    }

    private static void SetMultiHookMax(CommandArgs args)
    {
        if (args.Parameters.Count < 2 || !uint.TryParse(args.Parameters[1], out var max))
        {
            args.Player.SendErrorMessage("Usage: /afa duo <number>");
            return;
        }

        Configuration.Instance.GlobalMultiHookMaxNum = (int)max;
        Configuration.Instance.Save();
        args.Player.SendSuccessMessage($"Multi-hook limit set to: [c/92C5EC:{max}] hooks.");
    }

    private static void AddFishItem(CommandArgs args)
    {
        if (args.Parameters.Count < 4 || 
            !uint.TryParse(args.Parameters[2], out var count) ||
            !uint.TryParse(args.Parameters[3], out var minutes))
        {
            args.Player.SendErrorMessage("Usage: /afa add <item> <count> <minutes>");
            return;
        }

        var item = SelectItem(args.Player, args.Parameters[1]);
        if (item == null) return;

        Configuration.Instance.BaitRewards[item.type] = new Configuration.BaitReward
        {
            Count = (int)count,
            Minutes = (int)minutes
        };

        Configuration.Instance.Save();
        args.Player.SendSuccessMessage($"Set bait [i:{item.type}] rule: every {count} → {minutes} minutes");
    }

    private static void SetTime(CommandArgs args)
    {
        if (args.Parameters.Count < 3 || !uint.TryParse(args.Parameters[2], out var minutes))
        {
            args.Player.SendErrorMessage("Usage: /afa time <item> <minutes>");
            return;
        }

        var item = SelectItem(args.Player, args.Parameters[1]);
        if (item == null) return;

        if (!Configuration.Instance.BaitRewards.TryGetValue(item.type, out var bait))
        {
            args.Player.SendErrorMessage("Item is not in the bait list!");
            return;
        }

        bait.Minutes = (int)minutes;
        Configuration.Instance.Save();
        args.Player.SendSuccessMessage($"Updated bait [i:{item.type}] rule: every {bait.Count} → {minutes} minutes");
    }

    private static void ShowHelp(CommandArgs args)
    {
        args.Player.SendSuccessMessage("[Auto Fishing - Admin Commands]");
        args.Player.SendInfoMessage("/afa fish -- toggle global auto fishing");
        args.Player.SendInfoMessage("/afa buff -- toggle global buff");
        args.Player.SendInfoMessage("/afa multi -- toggle multi-hook mode");
        args.Player.SendInfoMessage("/afa duo <num> -- set hook limit");
        args.Player.SendInfoMessage("/afa mod -- toggle consumption mode");
        args.Player.SendInfoMessage("/afa set <item> <count> <minutes> -- set bait rule");
        args.Player.SendInfoMessage("/afa time <item> <minutes> -- change duration");
        args.Player.SendInfoMessage("/afa del <item> -- remove bait");
        args.Player.SendInfoMessage("/afa monster -- toggle monster catching");
        args.Player.SendInfoMessage("/afa anim -- toggle skip animation");
        args.Player.SendInfoMessage("/afa debug -- toggle debug mode");
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
            player.SendErrorMessage("Item does not exist!");
            return null;
        }

        return items[0];
    }
}