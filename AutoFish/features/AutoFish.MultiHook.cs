using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;

namespace AutoFish;

public partial class Plugin
{
    private void AddMultiHook(TSPlayer player, Projectile oldHook, Vector2 pos)
    {
        if (!Configuration.Instance.GlobalMultiHookFeatureEnabled) 
            return;

        var playerData = PlayerData.GetOrCreatePlayerData(player.Name, CreateDefaultPlayerData);
        
        if (!playerData.MultiHookEnabled) 
            return;

        // Count current active bobbers for this player
        var hookCount = Terraria.Main.projectile.Count(p => 
            p.active && 
            p.owner == oldHook.owner && 
            p.bobber);

        if (hookCount > Configuration.Instance.GlobalMultiHookMaxNum - 1) 
            return;
        
        if (hookCount > playerData.HookMaxNum - 1) 
            return;

        var guid = Guid.NewGuid().ToString();
        SpawnHook(player, oldHook, pos, guid);
    }
}