using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader;
using ReviveMod.Common.Configs;
using ReviveMod.Source.Common.Players;

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

    public class KeybindPlayer : ModPlayer
    {
        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.PauseRespawnTimer.JustPressed) {
                if (!ModContent.GetInstance<ReviveModConfig>().AllowTimerPausing) {
                    string respawnTimerPausingDisabled = Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerPausingDisabled");
                    Main.NewText(respawnTimerPausingDisabled, Color.Red);
                    return;
                }

                ref bool respawnTimerPaused = ref Player.GetModPlayer<ReviveModPlayer>().respawnTimerPaused;
                respawnTimerPaused = !respawnTimerPaused;
                string respawnTimerText = respawnTimerPaused ?
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerPaused") :
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerUnpaused");
                Main.NewText(respawnTimerText, ReviveMod.lifeGreen);
            }
        }
    }
}
