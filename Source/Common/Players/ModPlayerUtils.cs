using Microsoft.Xna.Framework;
using ReviveMod.Common.Configs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public partial class ReviveModPlayer : ModPlayer
    {
        public Vector2 LastDeathCenter
        {
            get {
                return new Vector2(
                    Player.lastDeathPostion.X + ((float)Player.width / 2),
                    Player.lastDeathPostion.Y + ((float)Player.height / 2)
                );
            }
            set {
                Player.lastDeathPostion = new Vector2(
                    value.X - ((float)Player.width / 2),
                    value.Y - ((float)Player.height / 2)
                );
            }
        }

        private bool RespawnTimerLegallyPaused => ModContent.GetInstance<ReviveModConfig>().ManualTimerPausing && respawnTimerPaused;

        private bool AvoidMaxTimerAndWholeSecond
            => timeSpentDead > 0 && Player.respawnTimer % 60 != 0;

        private bool HardcoreAndNotAllDeadForGood
        {
            get {
                if (Player.difficulty != PlayerDifficultyID.Hardcore) {
                    return false;
                }

                foreach (Player player in Main.player) {
                    if (!player.active) {
                        continue;
                    }

                    if (player.difficulty == PlayerDifficultyID.Hardcore && !player.dead) {
                        return true;
                    }
                }

                return false;
            }
        }

        private void SendKillPacket(int toClient = -1, int ignoreClient = -1)
        {
            ModPacket killPacket = Mod.GetPacket();
            killPacket.Write((byte)ReviveMod.MessageType.Kill);

            if (Main.netMode == NetmodeID.Server) {
                killPacket.Write((byte)Player.whoAmI);
            }

            killPacket.Send(toClient, ignoreClient);
        }

        private void SendRevivePacket(int toClient = -1, int ignoreClient = -1)
        {
            ModPacket revivePacket = Mod.GetPacket();
            revivePacket.Write((byte)ReviveMod.MessageType.Revive);

            if (Main.netMode == NetmodeID.Server) {
                revivePacket.Write((byte)Player.whoAmI);
            }

            revivePacket.Send(toClient, ignoreClient);
        }
    }
}
