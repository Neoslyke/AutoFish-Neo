using System.Globalization;
using System.Text;
using TShockAPI;

namespace AutoFish.Utils;

public static class Utils
{
    public static void DebugInfoLog(string message)
    {
        if (Plugin.DebugMode)
            TShock.Log.ConsoleInfo(message);
    }

    public static bool TrySwapValuableBaitToBack(
        TSPlayer player,
        int baitType,
        ICollection<int> valuableBaitIds,
        out int currentSlot,
        out int targetSlot,
        out int currentItemType,
        out int targetItemType)
    {
        currentSlot = -1;
        targetSlot = -1;
        currentItemType = 0;
        targetItemType = 0;

        if (player?.TPlayer?.inventory is null)
            return false;

        var inv = player.TPlayer.inventory;

        // Find current valuable bait slot
        for (var i = 0; i < inv.Length; i++)
        {
            if (inv[i].bait > 0 && inv[i].type == baitType)
            {
                currentSlot = i;
                currentItemType = inv[i].type;
                break;
            }
        }

        if (currentSlot == -1)
            return false;

        // Find last non-valuable bait slot
        for (var i = inv.Length - 1; i >= 0; i--)
        {
            if (inv[i].bait > 0 && !valuableBaitIds.Contains(inv[i].type))
            {
                targetSlot = i;
                targetItemType = inv[i].type;
                break;
            }
        }

        if (targetSlot == -1 || targetSlot == currentSlot)
            return false;

        // Swap items
        (inv[currentSlot], inv[targetSlot]) = (inv[targetSlot], inv[currentSlot]);
        return true;
    }

    public static void SendGradientMessage(
        TSPlayer player, 
        string text, 
        string startHex = "F3A6FF", 
        string endHex = "7CC7FF")
    {
        if (player == null || string.IsNullOrEmpty(text)) 
            return;

        var (sr, sg, sb) = ParseHex(startHex);
        var (er, eg, eb) = ParseHex(endHex);
        var len = text.Length;
        var msgBuilder = new StringBuilder(len * 12);

        for (var i = 0; i < len; i++)
        {
            var t = len <= 1 ? 0f : (float)i / (len - 1);
            var r = (int)MathF.Round(sr + ((er - sr) * t));
            var g = (int)MathF.Round(sg + ((eg - sg) * t));
            var b = (int)MathF.Round(sb + ((eb - sb) * t));

            msgBuilder.Append($"[c/{r:X2}{g:X2}{b:X2}:{text[i]}]");
        }

        player.SendInfoMessage(msgBuilder.ToString());
    }

    private static (int r, int g, int b) ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length != 6) 
            return (255, 255, 255);

        return (
            int.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture),
            int.Parse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture)
        );
    }

    public static int GetLimit(int playerCount) => playerCount switch
    {
        <= 5 => 100,
        <= 10 => 50,
        <= 20 => 25,
        _ => 10
    };
}