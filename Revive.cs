using Revive.Systems;
using System.IO;
using Terraria.ModLoader;

namespace Revive
{
	public class Revive : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI) => ModContent.GetInstance<ReviveSystem>().anyAlivePlayer = reader.ReadBoolean();
	}
}
