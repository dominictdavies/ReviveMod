using ReviveMod.Source.Common.Players;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Commands
{
    public class PauseCommand : ModCommand
    {
        public override CommandType Type
            => CommandType.Chat;

        public override string Command
            => "p";

        public override string Usage
            => "/p";

        public override string Description
            => "Pauses your respawn timer. Use this command again to resume it.";

        public override void Action(CommandCaller caller, string input, string[] args)
        {
            ReviveModPlayer callerModPlayer = caller.Player.GetModPlayer<ReviveModPlayer>();
            callerModPlayer.pausedRespawnTimer = !callerModPlayer.pausedRespawnTimer;
            caller.Reply(callerModPlayer.pausedRespawnTimer ? "Timer is now paused." : "Timer is now unpaused.", ReviveMod.lifeGreen);
        }
    }
}
