using Revive.ID;
using Terraria;
using Terraria.ID;
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
			Player[] playersToRevive;

			if (args.Length == 0) {
				playersToRevive = new Player[1];

				// Fill array with yourself
				playersToRevive[0] = GetExistingPlayer(caller.Player.name);

			} else {
				playersToRevive = new Player[args.Length];
				int count = 0;

				// Fill array with args
				foreach (string playerName in args) {
					Player player = GetExistingPlayer(playerName);
					playersToRevive[count++] = player ?? throw new UsageException(args[0] + " is not a player.");
				}
			}

			// Revive the players
			foreach (Player player in playersToRevive) {
				player.respawnTimer = 0;

				if (Main.netMode == NetmodeID.Server) {
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)PacketID.RevivePlayer);
					packet.Write((byte)player.whoAmI);
					packet.Send();
				}
			}
		}
	}
}
