using Microsoft.Xna.Framework;
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
    public class ReviveModPlayer : ModPlayer
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
            if (ReviveModDisabled() || Player.difficulty != PlayerDifficultyID.Hardcore) {
                return;
            }

            Player.difficulty = PlayerDifficultyID.SoftCore;
            usuallyHardcore = true;
        }

        public void ResetHardcorePlayer()
        {
            if (ReviveModDisabled() || !usuallyHardcore) {
                return;
            }

            Player.difficulty = PlayerDifficultyID.Hardcore;
            usuallyHardcore = false;
        }

        public void Kill()
        {
            PlayerDeathReason damageSource = PlayerDeathReason.ByCustomReason($"{Player.name} was killed.");
            int damage = Player.statLifeMax2;
            int hitDirection = 0;
            bool pvp = false;

            Player.KillMe(damageSource, damage, hitDirection, pvp);

            // KillMe designed for clients to call, so if called by server a net message must be manually sent
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendPlayerDeath(Player.whoAmI, damageSource, damage, hitDirection, pvp);
            }
        }

        public bool Revive(bool verbose = true)
        {
            // Revive failed
            if (!Player.dead || revived == true) {
                return false;
            }

            SaveHardcorePlayer();
            Player.Spawn(PlayerSpawnContext.ReviveFromDeath);

            // Makes player teleport to death location
            revived = true;

            // Revive succeeded
            if (verbose) {
                string playerRevived = Language.GetTextValue("Mods.ReviveMod.Chat.PlayerRevived");
                Main.NewText(string.Format(playerRevived, Player.name), ReviveMod.lifeGreen);
            }

            return true;
        }

        public void LocalTeleport()
        {
            // Move player to death position
            Player.Teleport(Player.lastDeathPostion - new Vector2(Player.width / 2, Player.height / 2), TeleportationStyleID.DebugTeleport);
            for (int i = 0; i < 50; i++) {
                double speed = 2d;
                double speedX = Main.rand.NextDouble() * speed * 2 - speed;
                double speedY = Math.Sqrt(speed * speed - speedX * speedX);

                if (Main.rand.NextBool()) {
                    speedY *= -1;
                }

                Dust.NewDust(Player.Center, 0, 0, DustID.Firework_Green, (float)speedX, (float)speedY);
            }

            // Reset flag
            revived = false;
        }

        public void SendRevivePlayer(int toClient = -1, int ignoreClient = -1)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)ReviveMod.MessageType.RevivePlayer);
            packet.Write((byte)Player.whoAmI);
            packet.Send(toClient, ignoreClient);
        }

        public void SendReviveTeleport(int toClient = -1, int ignoreClient = -1)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)ReviveMod.MessageType.ReviveTeleport);
            packet.Write((byte)Player.whoAmI);
            packet.Send(toClient, ignoreClient);
        }

        public static bool ReviveModDisabled()
            => !ModContent.GetInstance<ReviveModConfig>().Enabled || Main.netMode == NetmodeID.SinglePlayer;

        /* Called on client only, so use for UI */
        public override void OnEnterWorld()
            => timeSpentDead = 0;

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (ReviveModDisabled()) {
                return true;
            }

            SaveHardcorePlayer();
            return true;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (ReviveModDisabled()) {
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

        private bool HardcoreAndNotAllDeadForGood()
        {
            if (Player.difficulty != PlayerDifficultyID.Hardcore) {
                return false;
            }

            foreach (Player player in Main.player) {
                if (!player.active) {
                    continue;
                }

                if (player.difficulty == PlayerDifficultyID.Hardcore && !player.dead) {
                    return true;
                }
            }

            return false;
        }

        private bool AvoidMaxTimerAndWholeSecond()
            => timeSpentDead > 0 && Player.respawnTimer % 60 != 0;

        public override void UpdateDead()
        {
            if (ReviveModDisabled()) {
                return;
            }

            if ((respawnTimerPaused || CommonUtils.ActiveBossAlivePlayer() || HardcoreAndNotAllDeadForGood()) && AvoidMaxTimerAndWholeSecond()) {
                Player.respawnTimer++; // Undoes regular respawnTimer tick down
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
        {
            if (ReviveModDisabled()) {
                return;
            }

            timeSpentDead = 0;
            ResetHardcorePlayer();
        }

        public override void PreUpdate()
        {
            if (ReviveModDisabled()) {
                return;
            }

            /* Done this way as aura despawning does not call OnKill */
            if (Main.myPlayer == Player.whoAmI && !auraActive && oldAuraActive) {
                Revive();
            }

            oldAuraActive = auraActive;
            auraActive = false;

            // Teleport revived player to death location
            if (revived && !Player.dead && Player.position != Player.lastDeathPostion) {
                LocalTeleport();

                // Client declares teleport
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    SendReviveTeleport();
                }
            }

            if (Main.myPlayer == Player.whoAmI && Player.difficulty == PlayerDifficultyID.Hardcore && Player.ghost && !oldGhost) {
                Player.KillMeForGood();
            }

            oldGhost = Player.ghost;
        }
    }
}
