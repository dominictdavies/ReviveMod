using Revive.ID;
using Revive.Players;
using Revive.Systems;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Revive
{
	public class Revive : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketID id = (PacketID)reader.ReadByte();

			// TODO turn all cases into functions
			switch (id) {
				case PacketID.AlivePlayerCheck:
					bool anyAlivePlayer = reader.ReadBoolean();

					ModContent.GetInstance<ReviveSystem>().anyAlivePlayer = anyAlivePlayer;

					break;

				case PacketID.RevivePlayer:
					byte reviveWhoAmI = reader.ReadByte();

					// TODO turn into function
					Player respawningPlayer = Main.player[reviveWhoAmI];
					respawningPlayer.respawnTimer = 0;
					respawningPlayer.GetModPlayer<RevivePlayer>().revived = true;

					break;

				case PacketID.ReviveTeleport:
					if (Main.netMode == NetmodeID.MultiplayerClient)
						whoAmI = reader.ReadByte();

					// TODO turn into function
					Player teleportingPlayer = Main.player[whoAmI];
					teleportingPlayer.Center = teleportingPlayer.lastDeathPostion;
					teleportingPlayer.GetModPlayer<RevivePlayer>().revived = false;

					if (Main.netMode == NetmodeID.Server) {
						// TODO turn into function
						ModPacket packet = GetPacket();
						packet.Write((byte)PacketID.ReviveTeleport);
						packet.Write((byte)whoAmI);
						packet.Send(ignoreClient: whoAmI);
					}

					break;

				default:
					throw new Exception("Invalid packet ID.");
			}
		}
	}
}
