using ReviveMod.Source.Common.Projectiles;
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
            if (args.Length != 0) {
                throw new UsageException("No arguments were expected.");
            }

            ReviveAura.SetReviveTime(reviveTime);

            if (Main.netMode == NetmodeID.Server) {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)ReviveMod.MessageType.ChangeReviveTime);
                packet.Write(reviveTime);
                packet.Send();

                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"Revive time has been changed to {reviveTime} seconds."), ReviveMod.lifeGreen);
            } else {
                Main.NewText($"Revive time has been changed to {reviveTime} seconds.", ReviveMod.lifeGreen);
            }
        }
    }
}
