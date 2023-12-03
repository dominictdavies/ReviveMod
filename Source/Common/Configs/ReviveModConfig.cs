using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ReviveMod.Common.Configs
{
    public class ReviveModConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Reviving")]
        [DefaultValue(10)]
        public int ReviveTime;

        [Range(0f, 1f)]
        [Increment(0.1f)]
        [DefaultValue(0.5f)]
        public float NoBossMultiplier;

        [Header("Auras")]
        [Range(0f, 10f)]
        [Increment(0.25f)]
        [DefaultValue(2f)]
        public float MovementSpeed;
    }
}
