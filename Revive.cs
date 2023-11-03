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
		private void SetAnyAlivePlayer(bool anyAlivePlayer)
			=> ModContent.GetInstance<ReviveSystem>().anyAlivePlayer = anyAlivePlayer;

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketID id = (PacketID)reader.ReadByte();

			// TODO turn all cases into functions
			switch (id) {
				case PacketID.AlivePlayerCheck:
					bool anyAlivePlayer = reader.ReadBoolean();

					SetAnyAlivePlayer(anyAlivePlayer);

					break;

				case PacketID.RevivePlayer:
					byte reviveWhoAmI = reader.ReadByte();

					Player revivedPlayer = Main.player[reviveWhoAmI];
					revivedPlayer.GetModPlayer<RevivePlayer>().LocalRevive();

					break;

				case PacketID.ReviveTeleport:
					if (Main.netMode == NetmodeID.MultiplayerClient)
						whoAmI = reader.ReadByte();

					Player teleportingPlayer = Main.player[whoAmI];
					RevivePlayer modTeleportingPlayer = teleportingPlayer.GetModPlayer<RevivePlayer>();
					modTeleportingPlayer.LocalTeleport();

					if (Main.netMode == NetmodeID.Server)
						modTeleportingPlayer.SendReviveTeleport();

					break;

				default:
					throw new Exception("Invalid packet ID.");
			}
		}
	}
}
