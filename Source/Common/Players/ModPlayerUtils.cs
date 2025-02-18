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

        /* Max respawn timer shows a number larger than what you would expect it to be (i.e. a 10 second respawn shows 11 for 1st frame of being dead)
         * The Calamity Mod plays a ticking sound on the whole frame of the final three seconds before respawn
         * Hence the need for this function which avoids the maxed out respawn timer, and the repeating tick sound from the Calamity Mod
         */
        private bool AvoidMaxRespawnTimerAndWholeSecond
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

        private bool IsRespawnTimerPaused
        {
            get {
                var config = ModContent.GetInstance<ReviveModConfig>();

                return ((respawnTimerPausedManually && config.ManualRespawnTimerPausing)
                    || (HardcoreAndNotAllDeadForGood && config.HardcoreRespawnTimersWait)
                    || (CommonUtils.ActiveBossAlivePlayer && config.BossesPauseRespawnTimers))
                    && AvoidMaxRespawnTimerAndWholeSecond;
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
