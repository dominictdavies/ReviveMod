using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.ID;
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
            foreach (Player player in Main.player) {
                if (!player.active || player.dead) {
                    continue;
                }

                if (player.whoAmI == Projectile.owner) {
                    Projectile.timeLeft = -1;
                    break;
                }

                if (Projectile.Hitbox.Contains(player.Center.ToPoint())) {
                    Projectile.timeLeft--;
                }
            }

            Projectile.timeLeft++;
        }

        public override void OnKill(int timeLeft)
        {
            // Only other clients may revive owner
            if (Main.netMode == NetmodeID.Server || Main.myPlayer == Projectile.owner) {
                return;
            }

            Player owner = Main.player[Projectile.owner];
            if (owner.active && owner.dead) {
                owner.GetModPlayer<ReviveModPlayer>().Revive();
            }
        }
    }
}
