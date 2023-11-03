using Terraria;
using Terraria.ModLoader;

namespace Revive.Utils
{
	class UtilCommand
	{
		public static Player GetExistingPlayer(string playerName)
		{
			for (int i = 0; i < Main.maxNetPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && player.name == playerName)
					return player;
			}

			return null;
		}

		public static Player[] GetPlayersFromArgs(string[] args, Player caller)
		{
			Player[] playersFromArgs;

			if (args.Length == 0) {
				playersFromArgs = new Player[1];

				// Fill array with yourself
				playersFromArgs[0] = GetExistingPlayer(caller.name);

			} else {
				playersFromArgs = new Player[args.Length];
				int count = 0;

				// Fill array with args
				foreach (string playerName in args) {
					Player player = GetExistingPlayer(playerName);
					playersFromArgs[count++] = player ?? throw new UsageException($"{args[0]} is not a player.");
				}
			}

			return playersFromArgs;
		}
	}
}
