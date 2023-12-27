using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria.ModLoader;
using Terraria;

namespace ReviveMod.Source.Common.Players
{
    public class SpawnPlayer : ModPlayer
    {
        public override void Load()
            => IL_Player.Spawn += HookSpawn;

        private static void HookSpawn(ILContext il)
        {
            ILCursor c = new(il);

            try {
                c.GotoNext(MoveType.Before, i => i.MatchStsfld("Terraria.Main", "maxQ"));
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(SpawnAtReviveLocation);
            } catch (Exception exception) {
                ReviveMod reviveMod = ModContent.GetInstance<ReviveMod>();
                MonoModHooks.DumpIL(reviveMod, il);
                throw new ILPatchFailureException(reviveMod, il, exception);
            }
        }

        private static void SpawnAtReviveLocation(Player player)
        {
            ReviveModPlayer reviveModPlayer = player.GetModPlayer<ReviveModPlayer>();
            if (reviveModPlayer.IsTimeToRevive) {
                player.SpawnX = (int)(reviveModPlayer.LastDeathCenter.X / 16);
                player.SpawnY = (int)((player.lastDeathPostion.Y + player.height) / 16);
            }
        }
    }
}
