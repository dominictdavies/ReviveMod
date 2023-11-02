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
			int id = reader.ReadInt32();
			switch (id) {
				case 0:
					bool anyAlivePlayer = reader.ReadBoolean();
					ModContent.GetInstance<ReviveSystem>().anyAlivePlayer = anyAlivePlayer;
					break;
				case 1:
					int playerWhoAmI = reader.ReadInt32();
					Main.player[playerWhoAmI].respawnTimer = 0;
					break;
				default:
					throw new Exception("Invalid packet ID.");
			}
		}
	}
}
