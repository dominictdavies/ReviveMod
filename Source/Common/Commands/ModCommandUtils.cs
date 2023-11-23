using System.Collections.Generic;
using Terraria;

namespace ReviveMod.Source.Common.Commands
{
    public class ModCommandUtils
    {
        public static bool TryGetPlayer(string playerName, IEnumerable<Player> allPlayers, out Player player)
        {
            player = null;

            foreach (Player curPlayer in allPlayers) {
                if (curPlayer.active && curPlayer.name == playerName) {
                    player = curPlayer;
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<Player> GetPlayers(ICollection<string> playerNames, IEnumerable<Player> allPlayers, out string errorMessage)
        {
            errorMessage = null;

            List<Player> players = new();
            List<string> invalidPlayerNames = new();

            foreach (string playerName in playerNames) {
                if (TryGetPlayer(playerName, allPlayers, out Player player)) {
                    players.Add(player);
                } else {
                    invalidPlayerNames.Add(playerName);
                }
            }

            if (invalidPlayerNames.Count > 0) {
                string joinedPlayerNames = string.Join(", ", invalidPlayerNames);
                errorMessage = $"The following player name(s) are invalid: {joinedPlayerNames}.";
            }

            return players;
        }
    }
}
