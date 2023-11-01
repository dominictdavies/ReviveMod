using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Revive.Systems
{
    public class ReviveSystem : ModSystem
    {
        public bool oldAnyAlivePlayer;
        public bool anyAlivePlayer;

        public override void OnWorldLoad()
        {
            oldAnyAlivePlayer = true;
            anyAlivePlayer = true;
        }

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

        public override void PostUpdateWorld()
        {
            if (Main.netMode != NetmodeID.Server)
                return;

            if (anyAlivePlayer != oldAnyAlivePlayer) {
                ModPacket packet = Mod.GetPacket();
                packet.Write(anyAlivePlayer);
                packet.Send();
            }

            oldAnyAlivePlayer = anyAlivePlayer;
            UpdateAnyAlivePlayer();
        }
    }
}
