using ReviveMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.light = 1f;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            foreach (Player player in Main.player)
            {
                if (!player.active || player.dead)
                {
                    continue;
                }

                if (player.whoAmI == Projectile.owner)
                {
                    Projectile.timeLeft = -1;
                    break;
                }

                if (Projectile.Hitbox.Contains(player.Center.ToPoint()))
                {
                    Projectile.timeLeft--;
                }
            }

            Projectile.timeLeft++;
        }

        public override void OnKill(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.active && owner.dead)
            {
                owner.GetModPlayer<RevivePlayer>().Revive();
            }
        }
    }
}
