using ReviveMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Content.Buffs
{
    public class WearyDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<WearyDebuffPlayer>().wearyDebuff = true;
        }
    }

    public class WearyDebuffPlayer : ModPlayer
    {
        public bool wearyDebuff;

        public override void ResetEffects()
        {
            wearyDebuff = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (wearyDebuff) {
                ref StatModifier allDamage = ref Player.GetDamage(DamageClass.Generic);
                allDamage *= ModContent.GetInstance<ReviveModConfig>().DamageMultiplier;
            }
        }
    }
}
