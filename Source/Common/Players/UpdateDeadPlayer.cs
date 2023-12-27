using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public class UpdateDeadPlayer : ModPlayer
    {
        public override void Load()
            => IL_Player.UpdateDead += HookUpdateDead;

        private static void HookUpdateDead(ILContext il)
        {
            ILCursor c = new(il);

            try {
                c.Index--;
                c.GotoPrev(MoveType.After, i => i.MatchStfld("Terraria.Player", "ghost"));
                c.EmitLdarg0();
                c.EmitCall(typeof(Player).GetMethod("KillMeForGood", BindingFlags.Instance | BindingFlags.Public));
            } catch (Exception exception) {
                ReviveMod reviveMod = ModContent.GetInstance<ReviveMod>();
                MonoModHooks.DumpIL(reviveMod, il);
                throw new ILPatchFailureException(reviveMod, il, exception);
            }
        }
    }
}
