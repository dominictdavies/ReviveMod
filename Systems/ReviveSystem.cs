using Revive.ID;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Revive.Systems
{
	public class ReviveSystem : ModSystem
	{
		public bool oldAnyAlivePlayer = true;
		public bool anyAlivePlayer = true;

		private void UpdateAnyAlivePlayer()
		{
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && !player.dead) {
					anyAlivePlayer = true;
					return;
				}
			}

			anyAlivePlayer = false;
		}

		public override void OnWorldLoad()
		{
			oldAnyAlivePlayer = true;
			anyAlivePlayer = true;
		}

		public override void PostUpdateWorld()
		{
			// Server has final say on anyAlivePlayer
			if (Main.netMode != NetmodeID.Server)
				return;

			// Send only when anyAlivePlayer changes
			if (anyAlivePlayer != oldAnyAlivePlayer) {
				// TODO turn into function
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)PacketID.AlivePlayerCheck);
				packet.Write(anyAlivePlayer);
				packet.Send();
			}

			oldAnyAlivePlayer = anyAlivePlayer;
			UpdateAnyAlivePlayer();
		}
	}
}
