using Microsoft.Xna.Framework;
using Revive.ID;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
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
			=> "/revive [deadPlayer1 ...]" +
			"\n Providing no args will revive yourself.";

		public override string Description
			=> "Revives players for debug purposes";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Player[] playersToRevive;

			if (args.Length == 0) {
				playersToRevive = new Player[1];

				// Fill array with yourself
				playersToRevive[0] = Revive.GetExistingPlayer(caller.Player.name);

			} else {
				playersToRevive = new Player[args.Length];
				int count = 0;

				// Fill array with args
				foreach (string playerName in args) {
					Player player = Revive.GetExistingPlayer(playerName);
					playersToRevive[count++] = player ?? throw new UsageException($"{args[0]} is not a player.");
				}
			}

			// Check if any players are already alive
			foreach (Player player in playersToRevive)
				if (!player.dead)
					throw new UsageException($"{player.name} is already alive.");

			// Revive the players
			foreach (Player player in playersToRevive) {
				player.respawnTimer = 0;

				if (Main.netMode == NetmodeID.Server) {
					ModPacket packet = Mod.GetPacket();
					packet.Write((byte)PacketID.RevivePlayer);
					packet.Write((byte)player.whoAmI);
					packet.Send();
				}

				// Announce in chat
				if (Main.netMode == NetmodeID.Server)
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"{player.name} was revived!"), new Color(52, 235, 73));
				else
					Main.NewText($"{player.name} was revived!", new Color(52, 235, 73));
			}
		}
	}
}
