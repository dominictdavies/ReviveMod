using ReviveMod.Players;
using ReviveMod.Utils;
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
            Player[] playersToRevive = UtilCommand.GetPlayersFromArgs(args, caller.Player);

            // Check if any players are already alive
            foreach (Player player in playersToRevive)
                if (!player.dead)
                    throw new UsageException($"{player.name} is already alive.");

            // Revive the players
            foreach (Player player in playersToRevive)
                player.GetModPlayer<RevivePlayer>().Revive();
        }
    }
}
