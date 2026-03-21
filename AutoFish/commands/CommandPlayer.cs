using AutoFish.Utils;
using LazyAPI.Attributes;
using TShockAPI;

namespace AutoFish;

[Command("af")]
[Permissions("autofish")]
internal static class CommandPlayer
{
    public static void Help(CommandArgs args)
    {
        args.Player.SendSuccessMessage(GetString("[Auto Fishing]"));
        args.Player.SendSuccessMessage(GetString("/af help -- show auto fishing menu"));
        args.Player.SendSuccessMessage(GetString("/af status -- view your status"));
        args.Player.SendSuccessMessage(GetString("/af fish -- toggle auto fishing"));
        args.Player.SendSuccessMessage(GetString("/af buff -- toggle fishing buff"));
        args.Player.SendSuccessMessage(GetString("/af multi -- toggle multi-hook"));
        args.Player.SendSuccessMessage(GetString("/af hook <number> -- set personal hook limit"));
        args.Player.SendSuccessMessage(GetString("/af anim -- toggle skip fishing animation"));
        args.Player.SendSuccessMessage(GetString("/af quest -- toggle block quest fish"));
        args.Player.SendSuccessMessage(GetString("/af list -- list consumption items"));
        args.Player.SendSuccessMessage(GetString("/af rules [page] -- view custom fishing rules"));
        args.Player.SendSuccessMessage(GetString("/af bait -- toggle valuable bait protection"));
        args.Player.SendSuccessMessage(GetString("/af baitlist -- view valuable bait list"));
    }

    [Alias("status")]
    [RealPlayer]
    public static void Status(CommandArgs args)
    {
        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);

        SendGradientMessage(args.Player, GetString($"Auto Fishing: {(playerData.AutoFishEnabled ? "Enabled" : "Disabled")}"));
        SendGradientMessage(args.Player, GetString($"Buff: {(playerData.BuffEnabled ? "Enabled" : "Disabled")}"));
        SendGradientMessage(args.Player, GetString($"Multi-hook: {(playerData.MultiHookEnabled ? "Enabled" : "Disabled")}, Limit: {playerData.HookMaxNum}"));
        SendGradientMessage(args.Player, GetString($"Block Monsters: {(playerData.BlockMonsterCatch ? "Enabled" : "Disabled")}"));
        SendGradientMessage(args.Player, GetString($"Skip Animation: {(playerData.SkipFishingAnimation ? "Enabled" : "Disabled")}"));
        SendGradientMessage(args.Player, GetString($"Block Quest Fish: {(playerData.BlockQuestFish ? "Enabled" : "Disabled")}"));
        SendGradientMessage(args.Player, GetString($"Protect Valuable Bait: {(playerData.ProtectValuableBaitEnabled ? "Enabled" : "Disabled")}"));

        if (Configuration.Instance.BaitRewards.Count != 0 && playerData.CanConsume())
        {
            var (minutes, seconds) = playerData.GetRemainTime();
            SendGradientMessage(args.Player, GetString($"Consumption Mode: Active, Remaining: {minutes}m {seconds}s"));
        }
    }

    [Alias("fish")]
    [Permission("autofish.fish")]
    [RealPlayer]
    public static void Fish(CommandArgs args)
    {
        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.AutoFishEnabled = !playerData.AutoFishEnabled;
        args.Player.SendSuccessMessage(GetString($"Auto Fishing {(playerData.AutoFishEnabled ? "Enabled" : "Disabled")}"));
    }

    [Alias("buff")]
    [Permission("autofish.buff")]
    [RealPlayer]
    public static void Buff(CommandArgs args)
    {
        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.BuffEnabled = !playerData.BuffEnabled;
        args.Player.SendSuccessMessage(GetString($"Fishing Buff {(playerData.BuffEnabled ? "Enabled" : "Disabled")}"));
    }

    [Alias("multi")]
    [Permission("autofish.multihook")]
    [RealPlayer]
    public static void Multihook(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled)
        {
            args.Player.SendWarningMessage(GetString("Multi-hook is disabled by admin"));
            return;
        }

        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.MultiHookEnabled = !playerData.MultiHookEnabled;
        args.Player.SendSuccessMessage(GetString($"Multi-hook {(playerData.MultiHookEnabled ? "Enabled" : "Disabled")}"));
    }

    [Alias("monster")]
    [Permission("autofish.filter.monster")]
    [RealPlayer]
    public static void Monster(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalBlockMonsterCatch)
        {
            args.Player.SendWarningMessage(GetString("Monster filtering is disabled by admin"));
            return;
        }

        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.BlockMonsterCatch = !playerData.BlockMonsterCatch;
        args.Player.SendSuccessMessage(GetString($"Monster Catching {(playerData.BlockMonsterCatch ? "Blocked" : "Allowed")}"));
    }

    [Alias("anim")]
    [Permission("autofish.skipanimation")]
    [RealPlayer]
    public static void SkipFishingAnimation(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalSkipFishingAnimation)
        {
            args.Player.SendWarningMessage(GetString("Skip animation is disabled by admin"));
            return;
        }

        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.SkipFishingAnimation = !playerData.SkipFishingAnimation;
        args.Player.SendSuccessMessage(GetString($"Fishing Animation {(playerData.SkipFishingAnimation ? "Skipped" : "Normal")}"));
    }

    [Alias("list")]
    [RealPlayer]
    public static void List(CommandArgs args)
    {
        if (Configuration.Instance.BaitRewards.Count == 0)
        {
            args.Player.SendWarningMessage(GetString("No consumption items configured by admin"));
            return;
        }

        var msg = Configuration.Instance.BaitRewards
            .OrderByDescending(b => b.Value.Minutes)
            .Select(b => $"[i:{b.Key}] x{b.Value.Count} → {b.Value.Minutes} min");

        args.Player.SendInfoMessage(GetString("Consumption Items:"));
        args.Player.SendInfoMessage(string.Join("\n", msg));
    }

    [Alias("bait")]
    [RealPlayer]
    public static void Bait(CommandArgs args)
    {
        if (!Configuration.Instance.GlobalProtectValuableBaitEnabled)
        {
            args.Player.SendWarningMessage(GetString("Bait protection is disabled by admin"));
            return;
        }

        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.ProtectValuableBaitEnabled = !playerData.ProtectValuableBaitEnabled;
        args.Player.SendSuccessMessage(GetString($"Valuable Bait Protection {(playerData.ProtectValuableBaitEnabled ? "Enabled" : "Disabled")}"));
    }

    [Alias("baitlist")]
    [RealPlayer]
    public static void BaitList(CommandArgs args)
    {
        if (Configuration.Instance.ValuableBaitItemIds.Count == 0)
        {
            args.Player.SendWarningMessage(GetString("No valuable bait configured by admin"));
            return;
        }

        var msg = Configuration.Instance.ValuableBaitItemIds.Select(b => $"[i:{b}]");

        args.Player.SendInfoMessage(GetString("Valuable Bait List:"));
        args.Player.SendInfoMessage(string.Join(",", msg));
    }

    [Alias("hook")]
    [Permission("autofish.multihook")]
    [RealPlayer]
    public static void Multihook(CommandArgs args, uint max)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled)
        {
            args.Player.SendWarningMessage(GetString("Multi-hook is disabled by admin"));
            return;
        }

        if (max < 1)
        {
            args.Player.SendErrorMessage(GetString("Value must be at least 1"));
            return;
        }

        if (max > Configuration.Instance.GlobalMultiHookMaxNum)
        {
            args.Player.SendWarningMessage(GetString($"Value cannot exceed {Configuration.Instance.GlobalMultiHookMaxNum}"));
            return;
        }

        var playerData = Plugin.PlayerData.GetOrCreatePlayerData(args.Player.Name, Plugin.CreateDefaultPlayerData);
        playerData.HookMaxNum = (int)max;

        args.Player.SendSuccessMessage(GetString($"Hook limit set to {max} (global max {Configuration.Instance.GlobalMultiHookMaxNum})"));
    }
}