using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public class KillMePlayer : ModPlayer
    {
        public override void Load()
            => IL_Player.KillMe += HookKillMe;

        private static void HookKillMe(ILContext il)
        {
            ILCursor c = new(il);

            try {
                c.GotoNext(i => i.MatchLdfld("Terraria.Player", "difficulty"));
                c.Index--;
                ILLabel difficultyIf = c.MarkLabel();

                c.GotoNext(i => i.MatchLdcI4(0));
                ILLabel softcoreDeath = c.MarkLabel();

                c.GotoLabel(difficultyIf);

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(IsHardcoreRevival);
                c.Emit(OpCodes.Brtrue_S, softcoreDeath.Target);
            } catch (Exception exception) {
                ReviveMod reviveMod = ModContent.GetInstance<ReviveMod>();
                MonoModHooks.DumpIL(reviveMod, il);
                throw new ILPatchFailureException(reviveMod, il, exception);
            }
        }

        private static bool IsHardcoreRevival(Player player)
            => ReviveMod.Enabled && player.difficulty == PlayerDifficultyID.Hardcore;
    }
}
