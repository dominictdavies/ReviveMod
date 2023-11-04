﻿using Microsoft.Xna.Framework;
using ReviveMod.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Common.Commands
{
    public class KillCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

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
                playersToKill = ModCommandUtils.GetPlayers(args, caller);
			} else {
                playersToKill = new Player[] { caller.Player };
            }

            List<string> alreadyDeadPlayerNames = new();

            foreach (Player player in playersToKill) {
                if (!player.dead) {
                    player.GetModPlayer<RevivePlayer>().Kill();
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