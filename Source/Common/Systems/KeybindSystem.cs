using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Systems
{
    public class KeybindSystem : ModSystem
    {
        public static ModKeybind PauseRespawnTimer { get; private set; }

        public override void Load()
            => PauseRespawnTimer = KeybindLoader.RegisterKeybind(Mod, "PauseRespawnTimer", "P");

        public override void Unload()
            => PauseRespawnTimer = null;
    }
}
