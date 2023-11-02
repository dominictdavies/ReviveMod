using Revive.Players;
using Revive.Utils;
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
			Player[] playersToKill = UtilCommand.GetPlayersFromArgs(args, caller.Player);

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
