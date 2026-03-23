using TShockAPI;

namespace AutoFish;

internal static class CommandPlayer
{
    public static void HandleCommand(CommandArgs args)
    {
        if (!args.Player.RealPlayer)
        {
            args.Player.SendErrorMessage("This command requires a real player!");
            return;
        }

        var subcmd = args.Parameters.Count > 0 ? args.Parameters[0].ToLower() : "help";

        switch (subcmd)
        {
            case "status":
                ShowStatus(args);
                break;
            case "fish":
                ToggleFish(args);
                break;
            case "buff":
                ToggleBuff(args);
                break;
            case "multi":
                ToggleMultihook(args);
                break;
            case "monster":
                ToggleMonster(args);
                break;
            case "anim":
                ToggleSkipAnimation(args);
                break;
            case "quest":
                ToggleQuestFish(args);
                break;
            case "list":
                ShowList(args);
                break;
            case "bait":
                ToggleBait(args);
                break;
            case "baitlist":
                ShowBaitList(args);
                break;
            case "hook":
                SetHookLimit(args);
                break;
            default:
                ShowHelp(args);
                break;
        }
    }

    private static void ShowHelp(CommandArgs args)
    {
        args.Player.SendSuccessMessage("[Auto Fishing]");
        args.Player.SendInfoMessage("/af status -- view your status");
        args.Player.SendInfoMessage("/af fish -- toggle auto fishing");
        args.Player.SendInfoMessage("/af buff -- toggle fishing buff");
        args.Player.SendInfoMessage("/af multi -- toggle multi-hook");
        args.Player.SendInfoMessage("/af hook <number> -- set personal hook limit");
        args.Player.SendInfoMessage("/af anim -- toggle skip fishing animation");
        args.Player.SendInfoMessage("/af quest -- toggle block quest fish");
        args.Player.SendInfoMessage("/af list -- list consumption items");
        args.Player.SendInfoMessage("/af bait -- toggle valuable bait protection");
        args.Player.SendInfoMessage("/af baitlist -- view valuable bait list");
    }

    private static void ShowStatus(CommandArgs args)
    {
        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);

        args.Player.SendInfoMessage($"Auto Fishing: {(data.AutoFishEnabled ? "Enabled" : "Disabled")}");
        args.Player.SendInfoMessage($"Buff: {(data.BuffEnabled ? "Enabled" : "Disabled")}");
        args.Player.SendInfoMessage($"Multi-hook: {(data.MultiHookEnabled ? "Enabled" : "Disabled")}, Limit: {data.HookMaxNum}");
        args.Player.SendInfoMessage($"Block Monsters: {(data.BlockMonsterCatch ? "Enabled" : "Disabled")}");
        args.Player.SendInfoMessage($"Skip Animation: {(data.SkipFishingAnimation ? "Enabled" : "Disabled")}");
        args.Player.SendInfoMessage($"Block Quest Fish: {(data.BlockQuestFish ? "Enabled" : "Disabled")}");
        args.Player.SendInfoMessage($"Protect Valuable Bait: {(data.ProtectValuableBaitEnabled ? "Enabled" : "Disabled")}");

        if (Configuration.Instance.BaitRewards.Count != 0 && data.CanConsume())
        {
            var (minutes, seconds) = data.GetRemainTime();
            args.Player.SendInfoMessage($"Consumption Mode: Active, Remaining: {minutes}m {seconds}s");
        }
    }

    private static void ToggleFish(CommandArgs args)
    {
        if (!Plugin.HasFeaturePermission(args.Player, "fish"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.AutoFishEnabled = !data.AutoFishEnabled;
        args.Player.SendSuccessMessage($"Auto Fishing {(data.AutoFishEnabled ? "Enabled" : "Disabled")}");
    }

    private static void ToggleBuff(CommandArgs args)
    {
        if (!Plugin.HasFeaturePermission(args.Player, "buff"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.BuffEnabled = !data.BuffEnabled;
        args.Player.SendSuccessMessage($"Fishing Buff {(data.BuffEnabled ? "Enabled" : "Disabled")}");
    }

    private static void ToggleMultihook(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled)
        {
            args.Player.SendWarningMessage("Multi-hook is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "multihook"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.MultiHookEnabled = !data.MultiHookEnabled;
        args.Player.SendSuccessMessage($"Multi-hook {(data.MultiHookEnabled ? "Enabled" : "Disabled")}");
    }

    private static void ToggleMonster(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalBlockMonsterCatch)
        {
            args.Player.SendWarningMessage("Monster filtering is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "filter.monster"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.BlockMonsterCatch = !data.BlockMonsterCatch;
        args.Player.SendSuccessMessage($"Monster Catching {(data.BlockMonsterCatch ? "Blocked" : "Allowed")}");
    }

    private static void ToggleSkipAnimation(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalSkipFishingAnimation)
        {
            args.Player.SendWarningMessage("Skip animation is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "skipanimation"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.SkipFishingAnimation = !data.SkipFishingAnimation;
        args.Player.SendSuccessMessage($"Fishing Animation {(data.SkipFishingAnimation ? "Skipped" : "Normal")}");
    }

    private static void ToggleQuestFish(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalBlockQuestFish)
        {
            args.Player.SendWarningMessage("Quest fish blocking is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "filter.quest"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.BlockQuestFish = !data.BlockQuestFish;
        args.Player.SendSuccessMessage($"Quest Fish {(data.BlockQuestFish ? "Blocked" : "Allowed")}");
    }

    private static void ShowList(CommandArgs args)
    {
        if (Configuration.Instance.BaitRewards.Count == 0)
        {
            args.Player.SendWarningMessage("No consumption items configured by admin");
            return;
        }

        var msg = Configuration.Instance.BaitRewards
            .OrderByDescending(b => b.Value.Minutes)
            .Select(b => $"[i:{b.Key}] x{b.Value.Count} → {b.Value.Minutes} min");

        args.Player.SendInfoMessage("Consumption Items:");
        args.Player.SendInfoMessage(string.Join("\n", msg));
    }

    private static void ToggleBait(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalProtectValuableBaitEnabled)
        {
            args.Player.SendWarningMessage("Bait protection is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "bait.protect"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.ProtectValuableBaitEnabled = !data.ProtectValuableBaitEnabled;
        args.Player.SendSuccessMessage($"Valuable Bait Protection {(data.ProtectValuableBaitEnabled ? "Enabled" : "Disabled")}");
    }

    private static void ShowBaitList(CommandArgs args)
    {
        if (Configuration.Instance.ValuableBaitItemIds.Count == 0)
        {
            args.Player.SendWarningMessage("No valuable bait configured by admin");
            return;
        }

        var msg = Configuration.Instance.ValuableBaitItemIds.Select(b => $"[i:{b}]");
        args.Player.SendInfoMessage("Valuable Bait List:");
        args.Player.SendInfoMessage(string.Join(",", msg));
    }

    private static void SetHookLimit(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled)
        {
            args.Player.SendWarningMessage("Multi-hook is disabled by admin");
            return;
        }

        if (!Plugin.HasFeaturePermission(args.Player, "multihook"))
        {
            args.Player.SendErrorMessage("You don't have permission for this feature!");
            return;
        }

        if (args.Parameters.Count < 2 || !uint.TryParse(args.Parameters[1], out var max) || max < 1)
        {
            args.Player.SendErrorMessage("Usage: /af hook <number> (min 1)");
            return;
        }

        if (max > Configuration.Instance.GlobalMultiHookMaxNum)
        {
            args.Player.SendWarningMessage($"Value cannot exceed {Configuration.Instance.GlobalMultiHookMaxNum}");
            return;
        }

        var data = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        data.HookMaxNum = (int)max;
        args.Player.SendSuccessMessage($"Hook limit set to {max} (global max {Configuration.Instance.GlobalMultiHookMaxNum})");
    }
}