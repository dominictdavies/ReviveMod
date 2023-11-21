using ReviveMod.ID;
using ReviveMod.Players;
using ReviveMod.Systems;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod
{
	public class ReviveMod : Mod
	{
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
