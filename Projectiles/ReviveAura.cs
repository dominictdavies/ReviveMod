using Terraria;
using Terraria.ModLoader;

namespace Revive.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 64; // The width of projectile hitbox
            Projectile.height = 64; // The height of projectile hitbox
            Projectile.aiStyle = 0; // The ai style of the projectile, please reference the source code of Terraria
            Projectile.timeLeft = 600; // The live time for the projectile (60 = 1 second, so 600 is 10 seconds)
            Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
            Projectile.tileCollide = true; // Can the projectile collide with tiles?
        }

        public override void OnKill(int timeLeft)
        {
            // Revive player
        }
    }
}
