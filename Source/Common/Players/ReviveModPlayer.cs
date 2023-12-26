using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReviveMod.Common.Configs;
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
        public bool usuallyHardcore = false;
        public bool auraActive = false;
        public bool oldAuraActive = false;
        public bool oldGhost = false;
        public bool respawnTimerPaused = false;

        public void SaveHardcorePlayer()
        {
            if (!ReviveMod.Enabled || Player.difficulty != PlayerDifficultyID.Hardcore) {
                return;
            }

            Player.difficulty = PlayerDifficultyID.SoftCore;
            usuallyHardcore = true;
        }

        public void ResetHardcorePlayer()
        {
            if (!ReviveMod.Enabled || !usuallyHardcore) {
                return;
            }

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

            // For moving respawn location and creating dust
            revived = true;

            SaveHardcorePlayer();
            Player.Spawn(PlayerSpawnContext.ReviveFromDeath);

            // Revive succeeded
            if (verbose) {
                string playerRevived = Language.GetTextValue("Mods.ReviveMod.Chat.PlayerRevived");
                Main.NewText(string.Format(playerRevived, Player.name), ReviveMod.lifeGreen);
            }

            return true;
        }

        /* Called on client only, so use for UI */
        public override void OnEnterWorld()
        {
            timeSpentDead = 0;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (!ReviveMod.Enabled) {
                return true;
            }

            SaveHardcorePlayer();
            return true;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            ResetHardcorePlayer();
            if (Main.myPlayer == Player.whoAmI) {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, new(0, 0), ModContent.ProjectileType<ReviveAura>(), 0, 0, Main.myPlayer);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.PauseRespawnTimer.JustPressed) {
                respawnTimerPaused = !respawnTimerPaused;
                string respawnTimerText = respawnTimerPaused ?
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerPaused") :
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerUnpaused");
                Main.NewText(respawnTimerText, ReviveMod.lifeGreen);
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
                ResetHardcorePlayer();
                SetSpawnReviveLocation();
                CreateReviveDust();
                revived = false;
            }

            timeSpentDead = 0;
        }

        private void SetSpawnReviveLocation()
        {
            if (Main.myPlayer != Player.whoAmI) {
                return;
            }

            Player.SpawnX = (int)Math.Round(Player.lastDeathPostion.X / 16);
            Player.SpawnY = (int)Math.Round(Player.lastDeathPostion.Y / 16) + 3; // Accounting for player height
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

        public override void PreUpdate()
        {
            if (!ReviveMod.Enabled) {
                return;
            }

            /* Done this way as aura despawning does not call OnKill */
            if (Main.myPlayer == Player.whoAmI && !auraActive && oldAuraActive) {
                Revive();
            }

            oldAuraActive = auraActive;
            auraActive = false;

            if (Main.myPlayer == Player.whoAmI && Player.difficulty == PlayerDifficultyID.Hardcore && Player.ghost && !oldGhost) {
                Player.KillMeForGood();
            }

            oldGhost = Player.ghost;
        }
    }
}
