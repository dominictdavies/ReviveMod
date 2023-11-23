using ReviveMod.Source.Common.Projectiles;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class ReviveTimeCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.World;

        public override string Command
            => "reviveTime";

        public override string Usage
            => "/reviveTime seconds";

        public override string Description
            => "Alters the time it takes to revives players.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            if (caller.Player.whoAmI != 0) {
                throw new UsageException("Only the host may use this command.");
            }

            if (args.Length != 1) {
                throw new UsageException("Exactly one argument was expected.");
            }

            byte reviveTime = byte.Parse(args[0]);
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
