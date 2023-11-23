using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class DisableReviveCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "disableRevive";

        public override string Usage
            => "/disableRevive";

        public override string Description
            => "Disables this mod.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != 0) {
                throw new UsageException("Only the host may use this command.");
            }

            if (args.Length != 0) {
                throw new UsageException("No arguments were expected.");
            }

            ReviveModPlayer.SetRevive(false);

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)ReviveMod.MessageType.SetRevive);
                packet.Write(false);
                packet.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Revive mod disabled."), ReviveMod.lifeGreen);
            } else {
                Main.NewText($"Revive mod disabled.", ReviveMod.lifeGreen);
            }
        }
    }
}
