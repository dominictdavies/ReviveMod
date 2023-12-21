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
        public bool respawnTimerPaused = false;

        public void Kill()
        {
            PlayerDeathReason playerWasKilled = PlayerDeathReason.ByCustomReason($"{Player.name} was killed.");
            int playerLife = Player.statLife;
            int noDirection = 0;
            bool noPvp = false;

            // Kill the player
            Player.KillMe(playerWasKilled, playerLife, noDirection, noPvp);

            // Server syncs up clients through NetMessage
            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendPlayerDeath(Player.whoAmI, playerWasKilled, playerLife, noDirection, noPvp);
            }
        }

        public bool Revive(bool verbose = true, bool broadcast = true)
        {
            // Revive failed
            if (!Player.dead || revived == true) {
                return false;
            }

            // Player will respawn next tick
            Player.respawnTimer = 0;
            if (Player.difficulty == PlayerDifficultyID.Hardcore) {
                // Will get reset upon respawn
                Player.difficulty = PlayerDifficultyID.SoftCore;
                usuallyHardcore = true;
            }

            // Makes player teleport to death location
            revived = true;

            if (broadcast && Main.netMode != NetmodeID.SinglePlayer) {
                SendRevivePlayer();
            }

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

        /* Called on client only, so use for UI */
        public override void OnEnterWorld()
            => timeSpentDead = 0; 

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (!ModContent.GetInstance<ReviveModConfig>().Enabled || Main.netMode == NetmodeID.SinglePlayer) {
                return;
            }

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
            if (!ModContent.GetInstance<ReviveModConfig>().Enabled || Main.netMode == NetmodeID.SinglePlayer) {
                return;
            }

            /* Done this way as aura despawning does not call OnKill */
            if (oldAuraActive && !auraActive && Main.myPlayer == Player.whoAmI) {
                Revive();
            }

            oldAuraActive = auraActive;
            auraActive = false;

            if ((respawnTimerPaused || CommonUtils.ActiveBossAlivePlayer() || HardcoreAndNotAllDeadForGood()) && AvoidMaxTimerAndWholeSecond()) {
                Player.respawnTimer++; // Undoes regular respawnTimer tick down
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
        {
            timeSpentDead = 0;

            if (usuallyHardcore) {
                Player.difficulty = PlayerDifficultyID.Hardcore;
            }
        }

        public override void PreUpdate()
        {
            // Teleport revived player to death location
            if (revived && !Player.dead && Player.position != Player.lastDeathPostion) {
                LocalTeleport();

                // Client declares teleport
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    SendReviveTeleport();
                }
            }
        }
    }
}
