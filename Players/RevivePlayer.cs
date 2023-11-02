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
		public int timeSpentDead; // Needed to fix visual issue on respawn timer

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

		public void Revive()
		{
			string playerWasRevived = $"{Player.name} was revived!";
			Color lifeGreen = new(52, 235, 73);

			// Revive the player
			Player.respawnTimer = 0;

			// Server sends packets to sync clients
			if (Main.netMode == NetmodeID.Server) {
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)PacketID.RevivePlayer);
				packet.Write((byte)Player.whoAmI);
				packet.Send();
			}

			// Announce in chat
			if (Main.netMode == NetmodeID.Server)
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(playerWasRevived), lifeGreen);
			else
				Main.NewText(playerWasRevived, lifeGreen);
		}

		private bool ActiveBossAndAlivePlayer()
			=> Main.CurrentFrameFlags.AnyActiveBossNPC && ModContent.GetInstance<ReviveSystem>().anyAlivePlayer && timeSpentDead > 0;

		public override void OnEnterWorld()
			=> timeSpentDead = 0;

		public override void OnRespawn()
			=> timeSpentDead = 0;

		public override void UpdateDead()
		{
			if (ActiveBossAndAlivePlayer())
				Player.respawnTimer++; // Undoes regular respawn timer tickdown

			timeSpentDead++;
		}
	}
}
