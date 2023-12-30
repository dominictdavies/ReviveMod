using ReviveMod.Source.Common.Systems;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common
{
    public class CommonUtils
    {
        public static bool ActiveBossAlivePlayer
            => Main.CurrentFrameFlags.AnyActiveBossNPC
            && ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer;
    }
}
