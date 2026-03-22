using LazyAPI.Attributes;
using LazyAPI.ConfigFiles;
using TShockAPI.Hooks;

namespace AutoFish;

internal class Configuration : JsonConfigBase<Configuration>
{
    public class BaitReward
    {
        [LocalizedPropertyName(CultureType.English, "Count")]
        public int Count { get; set; }

        [LocalizedPropertyName(CultureType.English, "Minutes")]
        public int Minutes { get; set; }
    }

    [LocalizedPropertyName(CultureType.English, "Enabled")]
    public bool Enabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "AutoFishEnabled")]
    public bool GlobalAutoFishFeatureEnabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultAutoFish")]
    public bool DefaultAutoFishEnabled { get; set; }

    [LocalizedPropertyName(CultureType.English, "BuffEnabled")]
    public bool GlobalBuffFeatureEnabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultBuff")]
    public bool DefaultBuffEnabled { get; set; }

    [LocalizedPropertyName(CultureType.English, "MultiHookEnabled")]
    public bool GlobalMultiHookFeatureEnabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "MultiHookLimit")]
    public int GlobalMultiHookMaxNum { get; set; } = 5;

    [LocalizedPropertyName(CultureType.English, "DefaultMultiHook")]
    public bool DefaultMultiHookEnabled { get; set; }

    [LocalizedPropertyName(CultureType.English, "BlockMonsters")]
    public bool GlobalBlockMonsterCatch { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultBlockMonsters")]
    public bool DefaultBlockMonsterCatch { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "SkipAnimation")]
    public bool GlobalSkipFishingAnimation { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultSkipAnimation")]
    public bool DefaultSkipFishingAnimation { get; set; } = false;

    [LocalizedPropertyName(CultureType.English, "BlockQuestFish")]
    public bool GlobalBlockQuestFish { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultBlockQuestFish")]
    public bool DefaultBlockQuestFish { get; set; } = false;

    [LocalizedPropertyName(CultureType.English, "ProtectBait")]
    public bool GlobalProtectValuableBaitEnabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "DefaultProtectBait")]
    public bool DefaultProtectValuableBaitEnabled { get; set; } = true;

    [LocalizedPropertyName(CultureType.English, "ConsumptionMode")]
    public bool GlobalConsumptionModeEnabled { get; set; }

    [LocalizedPropertyName(CultureType.English, "BaitRewards")]
    public Dictionary<int, BaitReward> BaitRewards { get; set; } = [];

    [LocalizedPropertyName(CultureType.English, "ValuableBaits")]
    public List<int> ValuableBaitItemIds { get; set; } = [];

    [LocalizedPropertyName(CultureType.English, "Buffs")]
    public Dictionary<int, int> BuffDurations { get; set; } = [];

    protected override string Filename => "AutoFish";

    protected override void SetDefault()
    {
        this.BaitRewards = new Dictionary<int, BaitReward>()
        {
            { 2002, new BaitReward { Count = 1, Minutes = 1 } },
            { 2675, new BaitReward { Count = 1, Minutes = 5 } },
            { 2676, new BaitReward { Count = 1, Minutes = 10 } },
            { 3191, new BaitReward { Count = 1, Minutes = 8 } },
            { 3194, new BaitReward { Count = 1, Minutes = 5 } }
        };

        this.ValuableBaitItemIds =
        [
            2673,
            1999,
            2436,
            2437,
            2438,
            2891,
            4340,
            2893,
            4362,
            4419,
            2895
        ];

        this.BuffDurations.Add(114, 1);
    }

    protected override void Reload(ReloadEventArgs args)
    {
        args.Player.SendSuccessMessage("[AutoFishing] Config reloaded successfully");
    }
}