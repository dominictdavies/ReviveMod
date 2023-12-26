using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReviveMod.Source.Common.Systems;
using ReviveMod.Source.Content.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public partial class ReviveModPlayer : ModPlayer
    {
        public int timeSpentDead = 0; // Needed to fix a visual issue
        public bool revived = false;
        private bool usuallyHardcore = false;
        public bool auraActive = false;
        public bool oldAuraActive = false;
        public bool oldGhost = false;
        public bool respawnTimerPaused = false;

        public void SaveHardcorePlayer()
        {
            Player.difficulty = PlayerDifficultyID.SoftCore;
            usuallyHardcore = true;
        }

        public void ResetHardcorePlayer()
        {
            Player.difficulty = PlayerDifficultyID.Hardcore;
            usuallyHardcore = false;
        }

        public bool Kill()
        {
            // Kill failed
            if (Player.dead) {
                return false;
            }

            // TODO fix killing non clients
            PlayerDeathReason reason = PlayerDeathReason.ByCustomReason($"{Player.name} was killed.");
            Player.KillMe(reason, Player.statLifeMax2, 0);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendPlayerDeath(Player.whoAmI, reason, Player.statLifeMax2, 0, false);
            }

            return true;
        }

        public bool Revive(bool verbose = true)
        {
            // Revive failed
            if (!Player.dead) {
                return false;
            }

            // For moving respawn location
            revived = true;
            if (Player.difficulty == PlayerDifficultyID.Hardcore) {
                SaveHardcorePlayer();
            }

            Player.Spawn(PlayerSpawnContext.ReviveFromDeath);
            if (usuallyHardcore) {
                ResetHardcorePlayer();
            }

            CreateReviveDust();
            if (verbose) {
                string playerRevived = Language.GetTextValue("Mods.ReviveMod.Chat.PlayerRevived");
                Main.NewText(string.Format(playerRevived, Player.name), ReviveMod.lifeGreen);
            }

            return true;
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

                Dust.NewDust(Player.Center, 0, 0, DustID.Firework_Green, (float)speedX, (float)speedY);
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

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!ReviveMod.Enabled) {
                return true;
            }

            if (Player.difficulty == PlayerDifficultyID.Hardcore) {
                SaveHardcorePlayer();
            }

            return true;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if (usuallyHardcore) {
                ResetHardcorePlayer();
            }

            if (Main.myPlayer == Player.whoAmI) {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, new(0, 0), ModContent.ProjectileType<ReviveAura>(), 0, 0, Main.myPlayer);
            }
        }

        public override void UpdateDead()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if ((respawnTimerPaused || CommonUtils.ActiveBossAlivePlayer || HardcoreAndNotAllDeadForGood) && AvoidMaxTimerAndWholeSecond) {
                Player.respawnTimer++; // Undoes regular respawnTimer tick down
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            if (revived) {
                revived = false;
                SetSpawnReviveLocation();
            }

            timeSpentDead = 0;
        }

        private void SetSpawnReviveLocation()
        {
            Player.SpawnX = (int)(LastDeathCenter.X / 16);
            Player.SpawnY = (int)((Player.lastDeathPostion.Y + Player.height) / 16);
        }

        public override void PreUpdate()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            /* Done this way as aura despawning does not call OnKill */
            if (!auraActive && oldAuraActive) {
                Revive();
            }

            oldAuraActive = auraActive;
            auraActive = false;

            if (Main.myPlayer == Player.whoAmI && Player.difficulty == PlayerDifficultyID.Hardcore && Player.ghost && !oldGhost) {
                Player.KillMeForGood(); // Deletes player file
            }

            oldGhost = Player.ghost;
        }
    }
}
