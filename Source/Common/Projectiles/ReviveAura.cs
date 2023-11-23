using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        private static int reviveTime = 10 * 60;
        private readonly int progressTextInterval = 1 * 60;
        private readonly int nameTextInterval = 1 * 60;

        public static void SetReviveTime(int reviveTimeSecs)
            => reviveTime = reviveTimeSecs * 60;

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.alpha = 128;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = reviveTime;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 2f, 0f, 2f);

            foreach (Player player in Main.player) {
                if (!player.active || player.dead) {
                    continue;
                }

                if (player.whoAmI == Projectile.owner) {
                    Projectile.timeLeft = 0;
                    return;
                }

                if (Projectile.Hitbox.Contains(player.Center.ToPoint())) {
                    Projectile.timeLeft--;

                    if (Projectile.ai[0]-- == 0) {
                        CombatText.NewText(player.getRect(), CombatText.HealLife, Projectile.timeLeft / 60 + 1, true);
                        Projectile.ai[0] = progressTextInterval;
                    }
                }
            }

            if (Projectile.ai[1]-- == 0) {
                CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Magenta, Main.player[Projectile.owner].name);
                Projectile.ai[1] = nameTextInterval;
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
            if (owner.active) {
                owner.GetModPlayer<ReviveModPlayer>().Revive();
            }
        }
    }
}
