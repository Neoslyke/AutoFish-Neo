using Newtonsoft.Json;
using TShockAPI;

namespace AutoFish;

internal class Configuration
{
    private static readonly string ConfigPath = Path.Combine(TShock.SavePath, "AutoFish.json");
    
    public static Configuration Instance { get; private set; } = new();

    public class BaitReward
    {
        public int Count { get; set; }
        public int Minutes { get; set; }
    }

    public bool Enabled { get; set; } = true;
    public bool GlobalAutoFishFeatureEnabled { get; set; } = true;
    public bool DefaultAutoFishEnabled { get; set; }
    public bool GlobalBuffFeatureEnabled { get; set; } = true;
    public bool DefaultBuffEnabled { get; set; }
    public bool GlobalMultiHookFeatureEnabled { get; set; } = true;
    public int GlobalMultiHookMaxNum { get; set; } = 5;
    public bool DefaultMultiHookEnabled { get; set; }
    public bool GlobalBlockMonsterCatch { get; set; } = true;
    public bool DefaultBlockMonsterCatch { get; set; } = true;
    public bool GlobalSkipFishingAnimation { get; set; } = true;
    public bool DefaultSkipFishingAnimation { get; set; }
    public bool GlobalBlockQuestFish { get; set; } = true;
    public bool DefaultBlockQuestFish { get; set; }
    public bool GlobalProtectValuableBaitEnabled { get; set; } = true;
    public bool DefaultProtectValuableBaitEnabled { get; set; } = true;
    public bool GlobalConsumptionModeEnabled { get; set; }
    
    public Dictionary<int, BaitReward> BaitRewards { get; set; } = new();
    public List<int> ValuableBaitItemIds { get; set; } = new();
    public Dictionary<int, int> BuffDurations { get; set; } = new();

    public static void Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                Instance = JsonConvert.DeserializeObject<Configuration>(json) ?? new Configuration();
            }
            else
            {
                Instance = new Configuration();
                Instance.SetDefault();
                Instance.Save();
            }
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[AutoFish] Config load error: {ex.Message}");
            Instance = new Configuration();
            Instance.SetDefault();
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"[AutoFish] Config save error: {ex.Message}");
        }
    }

    private void SetDefault()
    {
        BaitRewards = new Dictionary<int, BaitReward>
        {
            { 2002, new BaitReward { Count = 1, Minutes = 1 } },
            { 2675, new BaitReward { Count = 1, Minutes = 5 } },
            { 2676, new BaitReward { Count = 1, Minutes = 10 } },
            { 3191, new BaitReward { Count = 1, Minutes = 8 } },
            { 3194, new BaitReward { Count = 1, Minutes = 5 } }
        };

        ValuableBaitItemIds = new List<int>
        {
            2673, 1999, 2436, 2437, 2438, 2891, 4340, 2893, 4362, 4419, 2895
        };

        BuffDurations = new Dictionary<int, int> { { 114, 1 } };
    }
}