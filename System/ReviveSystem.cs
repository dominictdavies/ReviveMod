using Terraria.ModLoader;

namespace Revive.System
{
    public class ReviveSystem : ModSystem
    {
        public int alivePlayerCount;

        public override void OnWorldLoad() => alivePlayerCount = 0;

        public override void PostUpdatePlayers() => alivePlayerCount = 0;
    }
}
