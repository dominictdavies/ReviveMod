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
