﻿using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
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
            => "Revives players for debug purposes.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (Main.netMode == NetmodeID.Server && !NetMessage.DoesPlayerSlotCountAsAHost(caller.Player.whoAmI)) {
                throw new UsageException("Only a host may use this command.");
            }

            IEnumerable<Player> playersToRevive;

            if (args.Length > 0) {
                playersToRevive = ModCommandUtils.GetPlayers(args, Main.player, out string warning);
                if (warning != null) {
                    caller.Reply(warning, Color.Red);
                }
            } else {
                playersToRevive = new Player[] { caller.Player };
            }

            List<string> alreadyAlivePlayerNames = new();

            foreach (Player player in playersToRevive) {
                if (player.dead) {
                    player.GetModPlayer<ReviveModPlayer>().ReviveMe();
                } else {
                    alreadyAlivePlayerNames.Add(player.name);
                }
            }

            if (alreadyAlivePlayerNames.Count > 0) {
                string joinedPlayerNames = string.Join(", ", alreadyAlivePlayerNames);
                caller.Reply($"The following player(s) are already alive: {joinedPlayerNames}.", Color.Red);
            }
        }
    }
}
