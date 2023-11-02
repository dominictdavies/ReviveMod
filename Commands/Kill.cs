using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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
					playersToKill[count++] = player ?? throw new UsageException(args[0] + " is not a player.");
				}
			}

			// Kill the players
			foreach (Player player in playersToKill) {
				PlayerDeathReason damageSource = PlayerDeathReason.ByCustomReason($"{player.name} was killed.");
				int damage = player.statLife;
				int direction = 0;
				bool pvp = false;

				player.KillMe(damageSource, damage, direction, pvp);
				if (Main.netMode == NetmodeID.Server)
					NetMessage.SendPlayerDeath(player.whoAmI, damageSource, damage, direction, pvp);
			}
		}
	}
}
