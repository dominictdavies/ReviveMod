using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        private static readonly float size = 1f;
        private static readonly int timeToRevive = 5*60;
        private static readonly int nameTimerDefault = 1*60;

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.scale = size;
            Projectile.light = size;
            Projectile.timeLeft = timeToRevive;
            Projectile.ai[0] = nameTimerDefault;
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

            if (Projectile.ai[0]-- == 0) {
                CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Lime, Main.player[Projectile.owner].name);
                Projectile.ai[0] = nameTimerDefault;
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
