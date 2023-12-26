using ReviveMod.Common.Configs;
using ReviveMod.Source.Common;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Content.Buffs
{
    public class TransfusingDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<TransfusingDebuffPlayer>().transfusingDebuff = true;
            ModBuffUtils.RemoveDebuffIfNotInAura(player, ref buffIndex);
        }
    }

    public class TransfusingDebuffPlayer : ModPlayer
    {
        public bool transfusingDebuff;

        public override void ResetEffects()
        {
            transfusingDebuff = false;
        }

        public override void UpdateBadLifeRegen()
        {
            if (transfusingDebuff) {
                if (Player.lifeRegen > 0) {
                    Player.lifeRegen = 0;
                }

                // Life regen speed kept at a minimum
                Player.lifeRegenTime = 0;

                // Life regen is measured in 1/2 life per second so this effect causes 1/4 max life to be lost over 10 seconds
                var config = ModContent.GetInstance<ReviveModConfig>();
                int lifeDecay = Player.statLifeMax2 * config.DrainPercentage / (config.ReviveTime * 50);
                if (!CommonUtils.ActiveBossAlivePlayer && config.NoBossMultiplier > 0) {
                    lifeDecay = (int)(lifeDecay * (1f / config.NoBossMultiplier));
                }

                Player.lifeRegen -= lifeDecay;
            }
        }
    }
}
