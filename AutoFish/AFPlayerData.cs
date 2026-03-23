namespace AutoFish;

public class AFPlayerData
{
    private readonly Dictionary<string, ItemData> _items = new(StringComparer.OrdinalIgnoreCase);

    internal ItemData GetOrCreatePlayerData(string name, Func<string, ItemData> factory)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name is required.", nameof(name));

        if (!_items.TryGetValue(name, out var data) || data == null)
        {
            data = factory(name);
            _items[name] = data;
        }

        return data;
    }

    public class ItemData
    {
        public string Name { get; set; } = "";
        public bool AutoFishEnabled { get; set; } = true;
        public bool BuffEnabled { get; set; }
        public int HookMaxNum { get; set; } = 3;
        public bool MultiHookEnabled { get; set; }
        public bool FirstFishHintShown { get; set; }
        public bool BlockMonsterCatch { get; set; }
        public bool SkipFishingAnimation { get; set; } = true;
        public bool BlockQuestFish { get; set; } = true;
        public bool ProtectValuableBaitEnabled { get; set; } = true;
        public DateTime ConsumeOverTime { get; set; } = DateTime.Now;
        public DateTime ConsumeStartTime { get; set; }

        public bool CanConsume() => GetRemainTimeInMinute() > 0;

        public double GetRemainTimeInMinute() => (ConsumeOverTime - DateTime.Now).TotalMinutes;

        public (int minutes, int seconds) GetRemainTime()
        {
            var span = ConsumeOverTime - DateTime.Now;
            return ((int)Math.Max(span.TotalMinutes, 0), Math.Max(span.Seconds, 0));
        }
    }
}