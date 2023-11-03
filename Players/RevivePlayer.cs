using Microsoft.Xna.Framework;
using Revive.ID;
using Revive.Systems;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Revive.Players
{
	public class RevivePlayer : ModPlayer
	{
		public int timeSpentDead = 0; // Needed to fix a visual issue
		public bool revived = false;

		public void Kill()
		{
			PlayerDeathReason playerWasKilled = PlayerDeathReason.ByCustomReason($"{Player.name} was killed.");
			int playerLife = Player.statLife;
			int noDirection = 0;
			bool noPvp = false;

			// Kill the player
			Player.KillMe(playerWasKilled, playerLife, noDirection, noPvp);

			// Server syncs up clients through NetMessage
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendPlayerDeath(Player.whoAmI, playerWasKilled, playerLife, noDirection, noPvp);
		}

		public void LocalRevive()
		{
			// Player will respawn next tick
			Player.respawnTimer = 0;

			// Makes player teleport to death location
			revived = true;
		}

		public void LocalTeleport()
		{
			// Move player to death position
			Player.Center = Player.lastDeathPostion;

			// Reset flag
			revived = false;
		}

		private void SendRevivePlayer()
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)PacketID.RevivePlayer);
			packet.Write((byte)Player.whoAmI);
			packet.Send();
		}

		public void SendReviveTeleport()
		{
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)PacketID.ReviveTeleport);

			if (Main.netMode == NetmodeID.Server) {
				packet.Write((byte)Player.whoAmI);
				packet.Send(ignoreClient: Player.whoAmI);
			} else {
				packet.Send();
			}
		}

		public void Revive()
		{
			string playerWasRevived = $"{Player.name} was revived!";
			Color lifeGreen = new(52, 235, 73);

			// Revive the player
			LocalRevive();

			// Sync and announce in chat
			if (Main.netMode == NetmodeID.Server) {
				SendRevivePlayer();
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(playerWasRevived), lifeGreen);
			} else {
				Main.NewText(playerWasRevived, lifeGreen);
			}
		}

		private bool ActiveBossAlivePlayer()
			=> Main.CurrentFrameFlags.AnyActiveBossNPC
			&& ModContent.GetInstance<ReviveSystem>().anyAlivePlayer
			&& timeSpentDead > 0; // Prevents respawn timer showing incorrect number

		public override void OnEnterWorld()
		{
			timeSpentDead = 0;
			revived = false;
		}

		public override void UpdateDead()
		{
			if (ActiveBossAlivePlayer())
				Player.respawnTimer++; // Undoes regular respawn timer tickdown

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
				if (Main.netMode == NetmodeID.MultiplayerClient)
					SendReviveTeleport();
			}
		}
	}
}
