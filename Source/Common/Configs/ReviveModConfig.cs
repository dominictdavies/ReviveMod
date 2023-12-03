using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ReviveMod.Common.Configs
{
    public class ReviveModConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Auras")]
        // [Label("$Some.Key")] // A label is the text displayed next to the option. This should usually be a short description of what it does. By default all ModConfig fields and properties have an automatic label translation key, but modders can specify a specific translation key.
        // [Tooltip("$Some.Key")] // A tooltip is a description showed when you hover your mouse over the option. It can be used as a more in-depth explanation of the option. Like with Label, a specific key can be provided.
        [DefaultValue(10)] // This sets the configs default value.
        public int ReviveTime;

        [DefaultValue(2f)]
        public float MovementSpeed;
    }
}
