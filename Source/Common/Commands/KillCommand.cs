using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class KillCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Server;

        public override string Command
            => "kill";

        public override string Usage
            => "/kill [alivePlayer1 ...]" +
            "\n Providing no args will kill yourself.";

        public override string Description
            => "Kills players for debug purposes.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            IEnumerable<Player> playersToKill;

            if (args.Length > 0) {
                playersToKill = ModCommandUtils.GetPlayers(args, Main.player, out string warning);
                if (warning != null) {
                    caller.Reply(warning, Color.Red);
                }
            } else {
                playersToKill = new Player[] { caller.Player };
            }

            List<string> alreadyDeadPlayerNames = new();

            foreach (Player player in playersToKill) {
                if (!player.dead) {
                    ModPacket killMePacket = Mod.GetPacket();
                    killMePacket.Write((byte)ReviveMod.MessageType.KillMe);
                    killMePacket.Send(toClient: player.whoAmI);
                } else {
                    alreadyDeadPlayerNames.Add(player.name);
                }
            }

            if (alreadyDeadPlayerNames.Count > 0) {
                string joinedPlayerNames = string.Join(", ", alreadyDeadPlayerNames);
                caller.Reply($"The following player(s) are already dead: {joinedPlayerNames}.", Color.Red);
            }
        }
    }
}
