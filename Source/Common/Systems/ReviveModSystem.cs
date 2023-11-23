using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Systems
{
    public class ReviveModSystem : ModSystem
    {
        public bool oldAnyAlivePlayer = true;
        public bool anyAlivePlayer = true;

        private void SendAlivePlayerCheck()
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)ReviveMod.MessageType.AlivePlayerCheck);
            packet.Write(anyAlivePlayer);
            packet.Send();
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

        public override void OnWorldLoad()
            => anyAlivePlayer = oldAnyAlivePlayer = true;

        public override void PreUpdateWorld()
            => UpdateAnyAlivePlayer();

        public override void PostUpdateWorld()
        {
            // Server declares anyAlivePlayer
            if (Main.netMode != NetmodeID.Server) {
                return;
            }

            // Send only when anyAlivePlayer changes
            if (anyAlivePlayer != oldAnyAlivePlayer) {
                SendAlivePlayerCheck();
            }

            oldAnyAlivePlayer = anyAlivePlayer;
        }
    }
}
