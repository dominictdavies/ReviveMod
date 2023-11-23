using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class ReviveTimeCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "reviveTime";

        public override string Usage
            => "/reviveTime seconds";

        public override string Description
            => "Alters the time it takes to revives players";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            IEnumerable<Player> playersToRevive;

            if (args.Length > 0) {
                playersToRevive = ModCommandUtils.GetPlayers(args, Main.player, out string errorMessage);
                if (errorMessage != null) {
                    caller.Reply(errorMessage, Color.Red);
                }
            } else {
                playersToRevive = new Player[] { caller.Player };
            }

            List<string> alreadyAlivePlayerNames = new();

            foreach (Player player in playersToRevive) {
                if (player.dead) {
                    player.GetModPlayer<ReviveModPlayer>().Revive();
                } else {
                    alreadyAlivePlayerNames.Add(player.name);
                }
            }

            if (alreadyAlivePlayerNames.Count > 0) {
                string joinedPlayerNames = string.Join(", ", alreadyAlivePlayerNames);
                caller.Reply($"The following player(s) are already alive: {joinedPlayerNames}.", Color.Red);
            }
        }
    }
}
