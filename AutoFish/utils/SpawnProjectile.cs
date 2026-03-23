using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace AutoFish.Utils;

public static class SpawnProjectile
{
    public static int NewProjectile(
        IEntitySource spawnSource, 
        Vector2 position, 
        Vector2 velocity, 
        int type,
        int damage, 
        float knockBack, 
        int owner = -1, 
        float ai0 = 0f, 
        float ai1 = 0f, 
        float ai2 = 0f,
        int timeLeft = -1, 
        string uuid = "")
    {
        return NewProjectile(
            spawnSource, 
            position.X, 
            position.Y, 
            velocity.X, 
            velocity.Y, 
            type, 
            damage, 
            knockBack,
            owner, 
            ai0, 
            ai1, 
            ai2, 
            timeLeft, 
            uuid);
    }

    /// <summary>
    /// Creates a new fishing bobber projectile.
    /// Bobber types: 360–366, 381, 382 (normal), 760, 775 (special), 986–993 (glowing)
    /// </summary>
    public static int NewProjectile(
        IEntitySource spawnSource, 
        float x, 
        float y, 
        float speedX, 
        float speedY,
        int type, 
        int damage, 
        float knockBack, 
        int owner = -1, 
        float ai0 = 0f, 
        float ai1 = 0f, 
        float ai2 = 0f,
        int timeLeft = -1, 
        string uuid = "")
    {
        if (owner == -1)
            owner = Main.myPlayer;

        // Find an available projectile slot (search from end for better slot distribution)
        var index = 1000;
        for (var i = 999; i > 0; i--)
        {
            if (!Main.projectile[i].active)
            {
                index = i;
                break;
            }
        }

        // If no slot found, reuse oldest projectile
        if (index == 1000)
            index = Projectile.FindOldestProjectile();

        var projectile = Main.projectile[index];

        // Initialize projectile
        projectile.SetDefaults(type);
        projectile.position.X = x;
        projectile.position.Y = y;
        projectile.owner = owner;
        projectile.velocity.X = speedX;
        projectile.velocity.Y = speedY;
        projectile.damage = damage;
        projectile.knockBack = knockBack;
        projectile.identity = index;
        projectile.gfxOffY = 0f;
        projectile.stepSpeed = 1f;

        // Check water collision
        projectile.wet = Collision.WetCollision(projectile.position, projectile.width, projectile.height);
        if (projectile.ignoreWater)
            projectile.wet = false;

        projectile.honeyWet = Collision.honey;
        projectile.shimmerWet = Collision.shimmer;

        Main.projectileIdentity[owner, index] = index;

        projectile.FindBannerToAssociateTo(spawnSource);

        // Only process fishing bobber AI style (61)
        if (projectile.aiStyle != 61)
            return 0;

        projectile.ai[0] = ai0;
        projectile.ai[1] = ai1;
        projectile.ai[2] = ai2;

        // Handle special projectile types
        if (type > 0 && type < ProjectileID.Count)
        {
            if (ProjectileID.Sets.NeedsUUID[type])
                projectile.projUUID = projectile.identity;

            if (ProjectileID.Sets.StardustDragon[type])
            {
                var uuidRef = Main.projectile[(int)projectile.ai[0]].projUUID;
                if (uuidRef >= 0)
                    projectile.ai[0] = uuidRef;
            }
        }

        // Sync to clients if in multiplayer
        if (Main.netMode != 0 && owner == Main.myPlayer)
            NetMessage.SendData(27, -1, -1, null, index);

        if (owner == Main.myPlayer)
            Main.player[owner].TryUpdateChannel(projectile);

        if (timeLeft > 0)
            projectile.timeLeft = timeLeft;

        // Store UUID for tracking
        projectile.miscText = uuid;

        return index;
    }
}