using ReviveMod.Source.Common.Players;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class EnableReviveCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "enableRevive";

        public override string Usage
            => "/enableRevive";

        public override string Description
            => "Enables this mod.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != 0) {
                throw new UsageException("Only the host may use this command.");
            }

            if (args.Length != 0) {
                throw new UsageException("No arguments were expected.");
            }

            ReviveModPlayer.SetRevive(true);

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)ReviveMod.MessageType.SetRevive);
                packet.Write(true);
                packet.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Revive mod enabled."), ReviveMod.lifeGreen);
            } else {
                Main.NewText($"Revive mod enabled.", ReviveMod.lifeGreen);
            }
        }
    }
}
