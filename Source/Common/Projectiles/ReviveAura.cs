using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        private static readonly int timeToRevive = 5*60;
        private static readonly int progressTimer = 1*60;
        private static readonly int nameTimer = 1*60;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.timeLeft = timeToRevive;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 2f, 0f, 2f);

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

                    if (Projectile.ai[0]-- == 0) {
                        CombatText.NewText(player.getRect(), CombatText.HealLife, Projectile.timeLeft / 60 + 1, true);
                        Projectile.ai[0] = progressTimer;
                    }
                }
            }

            if (Projectile.ai[1]-- == 0) {
                CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Magenta, Main.player[Projectile.owner].name);
                Projectile.ai[1] = nameTimer;
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
