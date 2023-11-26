﻿using Microsoft.Xna.Framework;
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

        private Vector3 GetAuraColor()
        {
            float progress = 1f - (float)Projectile.timeLeft / reviveTime;

            float red;
            float green;
            float blue;

            if (progress < 1f / 3f) {
                red = 1f;
                green = 0f;
                blue = 1f - progress * 3f;
            } else if (progress < 2f / 3f) {
                red = 1f;
                green = (progress - 1f / 3f) * 3f;
                blue = 0f;
            } else {
                red = 1f - (progress - 2f / 3f) * 3f;
                green = 1f;
                blue = 0f;
            }

            return new(red, green, blue);
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
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector3 rgb = GetAuraColor() * 255;
            Color color = new((int)rgb.X, (int)rgb.Y, (int)rgb.Z, 128);
            Main.EntitySpriteDraw(texture, Projectile.position - Main.screenPosition, texture.Bounds, color, 0f, Vector2.Zero, Projectile.scale, SpriteEffects.None);
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, GetAuraColor());

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
