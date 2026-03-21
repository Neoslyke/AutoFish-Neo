using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace AutoFish;

public partial class Plugin
{
    public void AddMultiHook(TSPlayer player, Projectile oldHook, Vector2 pos)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled) return;

        // Get matching configuration from the data table using player name
        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        if (!playerData.MultiHookEnabled) return;

        var hookCount = Main.projectile.Count(p => p.active && p.owner == oldHook.owner && p.bobber); // Bobber count
        // Quantity check
        if (hookCount > Configuration.Instance.GlobalMultiHookMaxNum - 1) return;
        if (hookCount > playerData.HookMaxNum - 1) return;

        var guid = Guid.NewGuid().ToString();
        SpawnHook(player, oldHook, pos, guid);
    }
}