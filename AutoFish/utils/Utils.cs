using System.Globalization;
using System.Text;
using TShockAPI;

namespace AutoFish.Utils;

public static class Utils
{
    public static string GetString(string text)
    {
        return text;
    }

    public static void DebugInfoLog(string message)
    {
        if (Plugin.DebugMode)
        {
            TShock.Log.ConsoleInfo(message);
        }
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

        for (var i = 0; i < inv.Length; i++)
        {
            if (inv[i].bait <= 0 || inv[i].type != baitType)
                continue;

            currentSlot = i;
            currentItemType = inv[i].type;
            break;
        }

        if (currentSlot == -1)
            return false;

        for (var i = inv.Length - 1; i >= 0; i--)
        {
            if (inv[i].bait <= 0) continue;
            if (valuableBaitIds.Contains(inv[i].type)) continue;

            targetSlot = i;
            targetItemType = inv[i].type;
            break;
        }

        if (targetSlot == -1 || targetSlot == currentSlot)
            return false;

        (inv[currentSlot], inv[targetSlot]) = (inv[targetSlot], inv[currentSlot]);
        return true;
    }

    public static void SendGradientMessage(
        TSPlayer player,
        string text,
        string startHex = "F3A6FF",
        string endHex = "7CC7FF")
    {
        if (player == null || string.IsNullOrEmpty(text)) return;

        var (sr, sg, sb) = ParseHex(startHex);
        var (er, eg, eb) = ParseHex(endHex);

        var len = text.Length;
        var sbMsg = new StringBuilder(len * 12);

        for (var i = 0; i < len; i++)
        {
            var t = len <= 1 ? 0f : (float)i / (len - 1);

            var r = (int)MathF.Round(sr + ((er - sr) * t));
            var g = (int)MathF.Round(sg + ((eg - sg) * t));
            var b = (int)MathF.Round(sb + ((eb - sb) * t));

            sbMsg.Append("[c/");
            sbMsg.Append(r.ToString("X2"));
            sbMsg.Append(g.ToString("X2"));
            sbMsg.Append(b.ToString("X2"));
            sbMsg.Append(':');
            sbMsg.Append(text[i]);
            sbMsg.Append(']');
        }

        player.SendInfoMessage(sbMsg.ToString());
    }

    private static (int r, int g, int b) ParseHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return (255, 255, 255);

        hex = hex.TrimStart('#');

        if (hex.Length != 6)
            return (255, 255, 255);

        var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return (r, g, b);
    }

    public static int GetLimit(int plrs)
    {
        return plrs <= 5 ? 100 :
               plrs <= 10 ? 50 :
               plrs <= 20 ? 25 :
               10;
    }
}