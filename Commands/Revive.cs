﻿using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Revive.Commands
{
	public class ReviveCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.World;

		public override string Command
			=> "revive";

		public override string Usage
			=> "/revive [player1 ...]" +
			"\n Providing no args will revive yourself.";

		public override string Description
			=> "Revives players for debug purposes";

		// Returns null if the player does not exist
		private Player GetExistingPlayer(string playerName)
		{
			for (int i = 0; i < Main.maxNetPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && player.name == playerName)
					return player;
			}

			return null;
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player[] playersToKill;

			if (args.Length == 0) {
				playersToKill = new Player[1];

				// Fill array with yourself
				playersToKill[0] = GetExistingPlayer(caller.Player.name);

			} else {
				playersToKill = new Player[args.Length];

				// Fill array with args
				foreach (string playerName in args) {
					Player player = GetExistingPlayer(playerName);

					if (player == null)
						throw new UsageException(args[0] + " is not a player.");

					playersToKill[playersToKill.Length] = player;
				}
			}

			foreach (Player player in playersToKill) {
				player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} was killed."), player.statLife, 0);
			}
		}
	}
}
