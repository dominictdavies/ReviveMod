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
        private int _reviveTimerMax;
        private int _progressTextTimer;
        private int _nameTextTimer;

        private ref Player Owner => ref Main.player[Projectile.owner];
        private ref float ReviveTimer => ref Projectile.ai[0];

        private Vector3 GetAuraColor()
        {
            float progress = 1f - ReviveTimer / _reviveTimerMax;

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

        private void ApplyDebuffs(ReviveModConfig config, Player player)
        {
            if (config.DrainLife) {
                player.AddBuff(ModContent.BuffType<TransfusingDebuff>(), (int)ReviveTimer);
            }
            if (config.SlowPlayers) {
                player.AddBuff(ModContent.BuffType<StrainedDebuff>(), (int)ReviveTimer);
            }
            if (config.ReduceDamage) {
                player.AddBuff(ModContent.BuffType<WearyDebuff>(), (int)ReviveTimer);
            }
        }

        private void MoveAura(float movementSpeed)
        {
            float maxVelocity = movementSpeed;
            float acceleration = maxVelocity / 10f;
            if (Main.myPlayer == Projectile.owner) {
                if (Owner.controlLeft && Projectile.velocity.X > -maxVelocity) {
                    Projectile.velocity.X -= acceleration;
                }
                if (Owner.controlRight && Projectile.velocity.X < maxVelocity) {
                    Projectile.velocity.X += acceleration;
                }
                if (Owner.controlUp && Projectile.velocity.Y > -maxVelocity) {
                    Projectile.velocity.Y -= acceleration;
                }
                if (Owner.controlDown && Projectile.velocity.Y < maxVelocity) {
                    Projectile.velocity.Y += acceleration;
                }

                // Updates position for other clients
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    NetMessage.SendData(MessageID.SyncProjectile, number: Projectile.whoAmI);
                }
            }

            Owner.Center = Projectile.Center;
            Owner.lastDeathPostion = Projectile.Center;
        }

        private void ShowProgress(Player player)
            => CombatText.NewText(player.getRect(), CombatText.HealLife, (int)ReviveTimer / 60 + 1, dramatic: true);

        private void ShowName()
            => CombatText.NewText(new Rectangle((int)Projectile.Center.X, (int)Projectile.Center.Y, 0, 0), Color.Magenta, Owner.name);

        private void ProduceLight()
            => Lighting.AddLight(Projectile.Center, GetAuraColor());

        public override void SetDefaults()
        {
            ReviveModConfig config = ModContent.GetInstance<ReviveModConfig>();
            int reviveTimeSeconds = config.ReviveTime * 60;
            float noBossMultiplier = config.NoBossMultiplier;

            _reviveTimerMax = CommonUtils.ActiveBossAlivePlayer() ? reviveTimeSeconds : (int)(reviveTimeSeconds * noBossMultiplier);
            if (_reviveTimerMax <= 0) { // Players will not be instantly revived when _reviveTimerMax is 0
                _reviveTimerMax = 1;
            }
            _progressTextTimer = 0;
            _nameTextTimer = 0;

            Projectile.width = 128;
            Projectile.height = 128;
            Projectile.alpha = 255;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
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
            // Projectile.ai is set to 0's by default
            if (ReviveTimer == 0) {
                ReviveTimer = _reviveTimerMax;
            }

            if (!Owner.dead) {
                Projectile.Kill();
                return;
            }

            ReviveModConfig config = ModContent.GetInstance<ReviveModConfig>();
            MoveAura(config.MovementSpeed);

            foreach (Player player in Main.player) {
                if (!player.active || player.dead || !Projectile.Hitbox.Intersects(player.getRect())) {
                    continue;
                }

                ReviveTimer--;
                ApplyDebuffs(config, player);

                if (_progressTextTimer-- == 0) {
                    ShowProgress(player);
                    _progressTextTimer = 1 * 60;
                }
            }

            if (_nameTextTimer-- == 0) {
                ShowName();
                _nameTextTimer = 1 * 60;
            }

            if (config.ProduceLight) {
                ProduceLight();
            }

            if (ReviveTimer <= 0) {
                Projectile.Kill();
                return;
            }

            // Undoes regular timeLeft tick down
            Projectile.timeLeft++;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner) {
                Owner.GetModPlayer<ReviveModPlayer>().Revive();
            }
        }
    }
}
