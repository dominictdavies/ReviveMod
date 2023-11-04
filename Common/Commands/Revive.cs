using Microsoft.Xna.Framework;
using ReviveMod.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Common.Commands
{
    public class ReviveCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "revive";

        public override string Usage
            => "/revive [deadPlayer1 ...]" +
            "\n Providing no args will revive yourself.";

        public override string Description
            => "Revives players for debug purposes";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
			IEnumerable<Player> playersToRevive;

			if (args.Length > 0) {
				playersToRevive = ModCommandUtils.GetPlayers(args, caller);
			} else {
				playersToRevive = new Player[] { caller.Player };
			}

			List<string> alreadyAlivePlayerNames = new();

			foreach (Player player in playersToRevive) {
				if (player.dead) {
					player.GetModPlayer<RevivePlayer>().Revive();
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
