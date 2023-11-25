using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        private static int reviveTime = 10 * 60;
        private readonly int progressTextInterval = 1 * 60;
        private readonly int nameTextInterval = 1 * 60;
        private readonly float acceleration = 0.2f;
        private readonly float maxVelocity = 2f;

        public static void SetReviveTime(int reviveTimeSecs)
            => reviveTime = reviveTimeSecs * 60;

        private Color GetReviveColor()
        {
            byte alpha = 128;
            float progress = 1 - (float)Projectile.timeLeft / reviveTime;

            byte red;
            byte green;
            byte blue;

            if (progress < 1.0 / 3.0) {
                red = 255;
                green = 0;
                blue = (byte)(255 - progress * 3 * 255);
            } else if (progress < 2.0 / 3.0) {
                red = 255;
                green = (byte)((progress - 1.0 / 3.0) * 3 * 255);
                blue = 0;
            } else {
                red = (byte)(255 - (progress - 2.0 / 3.0) * 3 * 255);
                green = 255;
                blue = 0;
            }

            return new(red, green, blue, alpha);
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.alpha = 255;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = reviveTime;
        }

        public override void PostDraw(Color lightColor)
        {
            // R255 G000 B255 Purple
            // Decrease blue
            // R255 G000 B000 Red
            // Increase green
            // R255 G255 B000 Yellow
            // Decrease red
            // R000 G255 B000 Green

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Color color = GetReviveColor();
            Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition, texture.Bounds, color, 0f, Vector2.Zero, Projectile.scale, SpriteEffects.None);
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 2f, 0f, 2f);

            // Aura removal and timer decreasing
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

            // Player name text
            if (Projectile.ai[1]-- == 0) {
                CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Magenta, Main.player[Projectile.owner].name);
                Projectile.ai[1] = nameTextInterval;
            }

            // Keeps aura alive
            Projectile.timeLeft++;

            // Aura movement
            Player owner = Main.player[Projectile.owner];
            if (Main.myPlayer == Projectile.owner) {
                if (owner.controlLeft && Projectile.velocity.X > -maxVelocity) {
                    Projectile.velocity.X -= acceleration;
                }
                if (owner.controlRight && Projectile.velocity.X < maxVelocity) {
                    Projectile.velocity.X += acceleration;
                }
                if (owner.controlUp && Projectile.velocity.Y > -maxVelocity) {
                    Projectile.velocity.Y -= acceleration;
                }
                if (owner.controlDown && Projectile.velocity.Y < maxVelocity) {
                    Projectile.velocity.Y += acceleration;
                }
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncProjectile, number: Projectile.whoAmI);
                }
            }

            owner.Center = Projectile.Center;
            owner.lastDeathPostion = Projectile.Center;
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
