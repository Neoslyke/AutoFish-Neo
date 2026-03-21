namespace AutoFish;

public class AFPlayerData
{
    // Player data table (uses dictionary for fast lookup by player name)
    private Dictionary<string, ItemData> Items { get; } = new(StringComparer.OrdinalIgnoreCase);

    internal ItemData GetOrCreatePlayerData(string name, Func<string, ItemData> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Player name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(factory);

        if (!this.Items.TryGetValue(name, out var data) || data == null)
        {
            data = factory(name);
            this.Items[name] = data;
        }

        return data;
    }

    public class ItemData(
        string name = "",
        bool autoFishEnabled = true,
        bool buffEnabled = false,
        int hookMaxNum = 3,
        bool multiHookEnabled = false,
        bool firstFishHintShown = false,
        bool blockMonsterCatch = false,
        bool skipFishingAnimation = true,
        bool protectValuableBaitEnabled = true,
        bool blockQuestFish = true)
    {
        public bool CanConsume()
        {
            return this.GetRemainTimeInMinute() > 0;
        }

        public double GetRemainTimeInMinute()
        {
            var minutesHave = (this.ConsumeOverTime - DateTime.Now).TotalMinutes;
            return minutesHave;
        }

        public (int minutes, int seconds) GetRemainTime()
        {
            var timeSpan = this.ConsumeOverTime - DateTime.Now;
            return ((int)Math.Max(timeSpan.TotalMinutes, 0), Math.Max(timeSpan.Seconds, 0));
        }

        // Player name
        public string Name { get; set; } = name ?? "";

        // Main auto fishing toggle
        public bool AutoFishEnabled { get; set; } = autoFishEnabled;

        // Buff toggle
        public bool BuffEnabled { get; set; } = buffEnabled;

        // Maximum number of hooks
        public int HookMaxNum { get; set; } = hookMaxNum;

        // Multi-hook toggle
        public bool MultiHookEnabled { get; set; } = multiHookEnabled;

        // Whether the auto-fishing hint has been shown
        public bool FirstFishHintShown { get; set; } = firstFishHintShown;

        // Block catching monsters
        public bool BlockMonsterCatch { get; set; } = blockMonsterCatch;

        // Skip fishing animation
        public bool SkipFishingAnimation { get; set; } = skipFishingAnimation;

        // Block quest fish
        public bool BlockQuestFish { get; set; } = blockQuestFish;

        // Protect valuable bait
        public bool ProtectValuableBaitEnabled { get; set; } = protectValuableBaitEnabled;

        // Timer used for consumption mode expiration
        public DateTime ConsumeOverTime { get; set; } = DateTime.Now;

        // Timer used only for display (start time)
        public DateTime ConsumeStartTime { get; set; } = default;
    }
}