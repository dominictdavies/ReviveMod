using Revive.System;
using Terraria;
using Terraria.ModLoader;

namespace Revive.Player
{
    public class RevivePlayer : ModPlayer
    {
        public int deadTimer; // Fix for visual issue on respawn timer

        public override void OnEnterWorld() => deadTimer = 0;

        public override void OnRespawn() => deadTimer = 0;

        public override void PreUpdate()
        {
            if (!Player.dead)
                ModContent.GetInstance<ReviveSystem>().alivePlayerCount++;
        }

        private bool ActiveBossAndAlivePlayer() => Main.CurrentFrameFlags.AnyActiveBossNPC && ModContent.GetInstance<ReviveSystem>().alivePlayerCount > 0 && deadTimer > 0;

        public override void UpdateDead()
        {
            if (ActiveBossAndAlivePlayer())
                Player.respawnTimer++; // Undoes regular respawn timer tickdown

            deadTimer++;
        }
    }
}
