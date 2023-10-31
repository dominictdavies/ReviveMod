using Terraria;
using Terraria.ModLoader;

namespace Revive.System
{
    public class RevivePlayer : ModPlayer
    {
        public int deadTimer; // Fix for visual issue on respawn timer

        public override void OnEnterWorld() => deadTimer = 0;

        public override void OnRespawn() => deadTimer = 0;

        public override void UpdateDead()
        {
            if (Main.CurrentFrameFlags.AnyActiveBossNPC && deadTimer > 0)
                Player.respawnTimer++;

            deadTimer++;
        }
    }
}
