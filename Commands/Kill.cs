using Revive.Players;
using Terraria;
using Terraria.ModLoader;

namespace Revive.Commands
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
			Player[] playersToKill;

			if (args.Length == 0) {
				playersToKill = new Player[1];

				// Fill array with yourself
				playersToKill[0] = Revive.GetExistingPlayer(caller.Player.name);

			} else {
				playersToKill = new Player[args.Length];
				int count = 0;

				// Fill array with args
				foreach (string playerName in args) {
					Player player = Revive.GetExistingPlayer(playerName);
					playersToKill[count++] = player ?? throw new UsageException($"{args[0]} is not a player.");
				}
			}

			// Check if any players are already alive
			foreach (Player player in playersToKill)
				if (player.dead)
					throw new UsageException($"{player.name} is already dead.");

			// Kill the players
			foreach (Player player in playersToKill)
				player.GetModPlayer<RevivePlayer>().Kill();
		}
	}
}
