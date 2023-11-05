using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class ModCommandUtils
    {
		public static bool TryGetPlayer(string playerName, IEnumerable<Player> playerEnumerable, out Player player)
        {
            player = null;

            foreach (Player curPlayer in playerEnumerable)
            {
                if (curPlayer.active && curPlayer.name == playerName)
                {
                    player = curPlayer;
                    return true;
                }
            }

            return false;
        }

		public static IEnumerable<Player> GetPlayers(ICollection<string> playerNames, IEnumerable<Player> playerEnumerable, CommandCaller caller)
        {
            List<Player> players = new();
            List<string> invalidPlayerNames = new();

            foreach (string playerName in playerNames)
            {
                if (TryGetPlayer(playerName, playerEnumerable, out Player player))
                {
                    players.Add(player);
                }
                else
                {
                    invalidPlayerNames.Add(playerName);
                }
            }

            if (invalidPlayerNames.Count > 0)
            {
                string joinedPlayerNames = string.Join(", ", invalidPlayerNames);
                caller.Reply($"The following player name(s) are invalid: {joinedPlayerNames}.", Color.Red);
            }

            return players;
        }
    }
}
