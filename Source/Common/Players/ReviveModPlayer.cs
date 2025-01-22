using Microsoft.Xna.Framework;
using ReviveMod.Source.Content.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public partial class ReviveModPlayer : ModPlayer
    {
        public int timeSpentDead = 0; // Needed to fix a visual issue
        public bool respawnTimerPaused = false;
        public bool auraActive;
        public bool oldAuraActive;
        public bool spawnAtDeathLocation;

        public void KillMe(bool broadcast = true)
        {
            if (Main.myPlayer == Player.whoAmI) {
                Player.KillMe(PlayerDeathReason.ByCustomReason($"{Player.name} was killed."), Player.statLifeMax2, 0);
            }

            // Any desired custom effects can go here

            if (Main.netMode == NetmodeID.Server && !broadcast) {
                SendKillPacket(ignoreClient: Player.whoAmI);
            } else if (Main.netMode != NetmodeID.SinglePlayer && broadcast) {
                SendKillPacket();
            }
        }

        public void ReviveMe(bool broadcast = true)
        {
            if (Main.myPlayer == Player.whoAmI) {
                spawnAtDeathLocation = true;
                Player.Spawn(PlayerSpawnContext.ReviveFromDeath);
            }

            CreateReviveDust();
            string playerRevived = Language.GetTextValue("Mods.ReviveMod.Chat.PlayerRevived");
            Main.NewText(string.Format(playerRevived, Player.name), ReviveMod.lifeGreen);

            if (Main.netMode == NetmodeID.Server && !broadcast) {
                SendRevivePacket(ignoreClient: Player.whoAmI);
            } else if (Main.netMode != NetmodeID.SinglePlayer && broadcast) {
                SendRevivePacket();
            }
        }

        private void CreateReviveDust()
        {
            for (int i = 0; i < 50; i++) {
                double speed = 2d;
                double speedX = Main.rand.NextDouble() * speed * 2 - speed;
                double speedY = Math.Sqrt(speed * speed - speedX * speedX);

                if (Main.rand.NextBool()) {
                    speedY *= -1;
                }

                Dust.NewDust(LastDeathCenter, 0, 0, DustID.Firework_Green, (float)speedX, (float)speedY);
            }
        }

        /* Called on client only, so use for UI */
        public override void OnEnterWorld()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            timeSpentDead = 0;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if (Main.myPlayer == Player.whoAmI) {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, new Vector2(0, 0), ModContent.ProjectileType<ReviveAura>(), 0, 0, Main.myPlayer);
            }
        }

        public override void UpdateDead()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if ((RespawnTimerLegallyPaused || CommonUtils.ActiveBossAlivePlayer || HardcoreAndNotAllDeadForGood) && AvoidMaxTimerAndWholeSecond) {
                Player.respawnTimer++; // Undoes regular respawnTimer tick down
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            timeSpentDead = 0;
        }

        public override void PreUpdate()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if (Main.myPlayer == Player.whoAmI && IsTimeToRevive) {
                ReviveMe();
            }

            oldAuraActive = auraActive;
            auraActive = false;
        }

        /* Done this way as opposed to aura OnKill to catch aura despawning */
        public bool IsTimeToRevive
            => Player.dead && !Player.ghost && !auraActive && oldAuraActive;
    }
}
