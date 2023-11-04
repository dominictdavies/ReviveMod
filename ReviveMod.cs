using ReviveMod.Players;
using ReviveMod.Systems;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod
{
	public class ReviveMod : Mod
	{
		internal enum MessageType : byte
		{
			AlivePlayerCheck,
			RevivePlayer,
			ReviveTeleport
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			MessageType type = (MessageType)reader.ReadByte();

			switch (type) {
				case MessageType.AlivePlayerCheck:
					bool anyAlivePlayer = reader.ReadBoolean();

					ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer = anyAlivePlayer;

					break;

				case MessageType.RevivePlayer:
					byte reviveWhoAmI = reader.ReadByte();

					Player revivedPlayer = Main.player[reviveWhoAmI];
					revivedPlayer.GetModPlayer<RevivePlayer>().LocalRevive();

					break;

				case MessageType.ReviveTeleport:
					if (Main.netMode == NetmodeID.MultiplayerClient)
						whoAmI = reader.ReadByte();

					Player teleportingPlayer = Main.player[whoAmI];
					RevivePlayer modTeleportingPlayer = teleportingPlayer.GetModPlayer<RevivePlayer>();
					modTeleportingPlayer.LocalTeleport();

					if (Main.netMode == NetmodeID.Server)
						modTeleportingPlayer.SendReviveTeleport();

					break;

				default:
					Logger.WarnFormat("ReviveMod: Unknown message type: {0}", type);
					break;
			}
		}
	}
}
