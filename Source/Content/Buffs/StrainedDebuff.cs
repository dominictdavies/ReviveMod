using ReviveMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Content.Buffs
{
    public class StrainedDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<StrainedDebuffPlayer>().strainedDebuff = true;
        }
    }

    public class StrainedDebuffPlayer : ModPlayer
    {
        public bool strainedDebuff;

        public override void ResetEffects()
        {
            strainedDebuff = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (strainedDebuff) {
                Player.moveSpeed *= ModContent.GetInstance<ReviveModConfig>().SpeedMultiplier;
            }
        }
    }
}
