using ReviveMod.Source.Common.Systems;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common
{
    public class CommonUtils
    {
        private static readonly HashSet<short> manualBosses = [NPCID.EaterofWorldsHead];

        public static bool ActiveBossAlivePlayer
        {
            get {
                // EoW does not use the boss attribute, so they must be considered manually
                return (Main.CurrentFrameFlags.AnyActiveBossNPC || Main.npc.Any(npc => npc.active && manualBosses.Contains(npc.type))) 
                    && ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer;
            }
        }
    }
}
