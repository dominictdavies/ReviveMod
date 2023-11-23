using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class ToggleRevivingCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "toggleReviving";

        public override string Usage
            => "/toggleReviving";

        public override string Description
            => "Toggles whether or not revive auras should appear at all.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != 0) {
                throw new UsageException("Only the host may use this command.");
            }

            if (args.Length != 0) {
                throw new UsageException("No arguments were expected.");
            }

            bool revivingEnabled = ReviveModPlayer.ToggleReviving();

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)ReviveMod.MessageType.ToggleReviving);
                packet.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Reviving set to {revivingEnabled}."), ReviveMod.lifeGreen);
            } else {
                Main.NewText($"Reviving set to {revivingEnabled}.", ReviveMod.lifeGreen);
            }
        }
    }
}
