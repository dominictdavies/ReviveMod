using Revive.ID;
using Revive.Systems;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Revive
{
	public class Revive : Mod
	{
		/* Returns null if the player does not exist */
		public static Player GetExistingPlayer(string playerName)
		{
			for (int i = 0; i < Main.maxNetPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && player.name == playerName)
					return player;
			}

			return null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketID id = (PacketID)reader.ReadByte();
			switch (id) {
				case PacketID.AlivePlayerCheck:
					bool anyAlivePlayer = reader.ReadBoolean();
					ModContent.GetInstance<ReviveSystem>().anyAlivePlayer = anyAlivePlayer;
					break;
				case PacketID.RevivePlayer:
					byte reviveWhoAmI = reader.ReadByte();
					Main.player[reviveWhoAmI].respawnTimer = 0;
					break;
				default:
					throw new Exception("Invalid packet ID.");
			}
		}
	}
}
