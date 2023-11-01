using Revive.Systems;
using Terraria;
using Terraria.ModLoader;

namespace Revive.Players
{
	public class RevivePlayer : ModPlayer
	{
		public int deadTimer; // Fix for visual issue on respawn timer

		public override void OnEnterWorld() => deadTimer = 0;

		public override void OnRespawn() => deadTimer = 0;

		private bool ActiveBossAndAlivePlayer() => Main.CurrentFrameFlags.AnyActiveBossNPC && ModContent.GetInstance<ReviveSystem>().anyAlivePlayer && deadTimer > 0;

		public override void UpdateDead()
		{
			if (ActiveBossAndAlivePlayer())
				Player.respawnTimer++; // Undoes regular respawn timer tickdown

			deadTimer++;
		}
	}
}
