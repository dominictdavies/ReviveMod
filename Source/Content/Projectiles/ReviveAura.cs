using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReviveMod.Common.Configs;
using ReviveMod.Source.Common;
using ReviveMod.Source.Common.Players;
using ReviveMod.Source.Content.Buffs;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Content.Projectiles
{
    public class ReviveAura : ModProjectile
    {
        private int reviveTimerMax;
        private int progressTextTimer;
        private int nameTextTimer;

        private ref int ReviveTimer => ref Projectile.timeLeft;

        private Vector3 GetAuraColor()
        {
            float progress = 1f - (float)ReviveTimer / reviveTimerMax;

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
            ReviveModConfig config = ModContent.GetInstance<ReviveModConfig>();
            int reviveTimeSeconds = config.ReviveTime * 60;
            float noBossMultiplier = config.NoBossMultiplier;

            reviveTimerMax = CommonUtils.ActiveBossAlivePlayer() ? reviveTimeSeconds : (int)(reviveTimeSeconds * noBossMultiplier);
            if (reviveTimerMax <= 0) {
                reviveTimerMax = 1;
            }

            progressTextTimer = 0;
            nameTextTimer = 0;

            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.alpha = 255;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            ReviveTimer = reviveTimerMax;
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
            ReviveModConfig config = ModContent.GetInstance<ReviveModConfig>();
            if (config.ProduceLight) {
                Lighting.AddLight(Projectile.Center, GetAuraColor());
            }

            // Aura removal and timer decreasing
            foreach (Player player in Main.player) {
                if (!player.active || player.dead) {
                    continue;
                }

                if (player.whoAmI == Projectile.owner) {
                    ReviveTimer = 0;
                    return;
                }

                if (Projectile.Hitbox.Intersects(player.getRect()) && ReviveTimer > 0) {
                    ReviveTimer--;

                    // Apply balancing debuffs
                    if (config.DrainLife) {
                        player.AddBuff(ModContent.BuffType<TransfusingDebuff>(), ReviveTimer);
                    }
                    if (config.SlowPlayers) {
                        player.AddBuff(ModContent.BuffType<StrainedDebuff>(), ReviveTimer);
                    }
                    if (config.ReduceDamage) {
                        player.AddBuff(ModContent.BuffType<WearyDebuff>(), ReviveTimer);
                    }

                    if (progressTextTimer-- == 0) {
                        CombatText.NewText(player.getRect(), CombatText.HealLife, ReviveTimer / 60 + 1, dramatic: true);
                        progressTextTimer = 1 * 60;
                    }
                }
            }

            // Player name text
            if (nameTextTimer-- == 0) {
                CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Magenta, Main.player[Projectile.owner].name);
                nameTextTimer = 1 * 60;
            }

            // Keeps aura alive
            ReviveTimer++;

            // Aura movement
            float maxVelocity = config.MovementSpeed;
            float acceleration = maxVelocity / 10f;
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
            // Revive owner runs the revive
            if (Main.myPlayer != Projectile.owner) {
                return;
            }

            Player owner = Main.player[Main.myPlayer];
            owner.GetModPlayer<ReviveModPlayer>().Revive();
        }
    }
}
