using ReviveMod.Source.Common.Projectiles;
using ReviveMod.Source.Common.Systems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public class ReviveModPlayer : ModPlayer
    {
        private static bool revivingEnabled = true;
        public int timeSpentDead = 0; // Needed to fix a visual issue
        public bool revived = false;

        public static bool ToggleReviving()
        {
            revivingEnabled = !revivingEnabled;
            return revivingEnabled;
        }

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

            // Makes player teleport to death location
            revived = true;

            if (broadcast && Main.netMode != NetmodeID.SinglePlayer) {
                SendRevivePlayer();
            }

            // Revive succeeded
            if (!verbose) {
                return true;
            }

            string playerWasRevived = $"{Player.name} was revived!";
            Main.NewText(playerWasRevived, ReviveMod.lifeGreen);

            return true;
        }

        public void LocalTeleport()
        {
            // Move player to death position
            Player.Center = Player.lastDeathPostion;

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

        private bool ActiveBossAlivePlayer()
            => Main.CurrentFrameFlags.AnyActiveBossNPC
            && ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer
            && timeSpentDead > 0; // Prevents respawn timer showing incorrect number

        public override void OnEnterWorld()
        {
            timeSpentDead = 0;
            revived = false;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (revivingEnabled && Main.myPlayer == Player.whoAmI) {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, new(0, 0), ModContent.ProjectileType<ReviveAura>(), 0, 0, Main.myPlayer);
            }
        }

        public override void UpdateDead()
        {
            if (ActiveBossAlivePlayer()) {
                Player.respawnTimer++; // Undoes regular respawn timer tickdown
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
            => timeSpentDead = 0;

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
