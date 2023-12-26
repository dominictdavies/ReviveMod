using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReviveMod.Common.Configs;
using ReviveMod.Source.Common.Systems;
using ReviveMod.Source.Content.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ReviveMod.Source.Common.Players
{
    public class ReviveModPlayer : ModPlayer
    {
        public int timeSpentDead = 0; // Needed to fix a visual issue
        public Vector2 reviveLocation = Vector2.Zero;
        public bool usuallyHardcore = false;
        public bool auraActive = false;
        public bool oldAuraActive = false;
        public bool oldGhost = false;
        public bool respawnTimerPaused = false;

        public void SaveHardcorePlayer()
        {
            if (ReviveModDisabled() || Player.difficulty != PlayerDifficultyID.Hardcore) {
                return;
            }

            Player.difficulty = PlayerDifficultyID.SoftCore;
            usuallyHardcore = true;
        }

        public void ResetHardcorePlayer()
        {
            if (ReviveModDisabled() || !usuallyHardcore) {
                return;
            }

            Player.difficulty = PlayerDifficultyID.Hardcore;
            usuallyHardcore = false;
        }

        public bool Kill()
        {
            // Kill failed
            if (Player.dead) {
                return false;
            }

            // TODO fix killing non clients
            PlayerDeathReason reason = PlayerDeathReason.ByCustomReason($"{Player.name} was killed.");
            Player.KillMe(reason, Player.statLifeMax2, 0);

            if (Main.netMode == NetmodeID.Server) {
                NetMessage.SendPlayerDeath(Player.whoAmI, reason, Player.statLifeMax2, 0, false);
            }

            return true;
        }

        public bool Revive(bool verbose = true)
        {
            // Revive failed
            if (!Player.dead) {
                return false;
            }

            reviveLocation = Player.position;
            SaveHardcorePlayer();
            Player.Spawn(PlayerSpawnContext.ReviveFromDeath);

            // Revive succeeded
            if (verbose) {
                string playerRevived = Language.GetTextValue("Mods.ReviveMod.Chat.PlayerRevived");
                Main.NewText(string.Format(playerRevived, Player.name), ReviveMod.lifeGreen);
            }

            return true;
        }

        public static bool ReviveModDisabled()
            => !ModContent.GetInstance<ReviveModConfig>().Enabled || Main.netMode == NetmodeID.SinglePlayer;

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
            ref Vector2 reviveLocation = ref player.GetModPlayer<ReviveModPlayer>().reviveLocation;
            if (reviveLocation != Vector2.Zero) {
                player.SpawnX = (int)Math.Round(reviveLocation.X / 16);
                player.SpawnY = (int)Math.Round(reviveLocation.Y / 16) + 3; // Accounting for player height
                reviveLocation = Vector2.Zero;
            }
        }

        /* Called on client only, so use for UI */
        public override void OnEnterWorld()
            => timeSpentDead = 0;

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
        {
            if (ReviveModDisabled()) {
                return true;
            }

            SaveHardcorePlayer();
            return true;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (ReviveModDisabled()) {
                return;
            }

            ResetHardcorePlayer();
            if (Main.myPlayer == Player.whoAmI) {
                Projectile.NewProjectile(Player.GetSource_Death(), Player.Center, new(0, 0), ModContent.ProjectileType<ReviveAura>(), 0, 0, Main.myPlayer);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (KeybindSystem.PauseRespawnTimer.JustPressed) {
                respawnTimerPaused = !respawnTimerPaused;
                string respawnTimerText = respawnTimerPaused ?
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerPaused") :
                                          Language.GetTextValue("Mods.ReviveMod.Chat.RespawnTimerUnpaused");
                Main.NewText(respawnTimerText, ReviveMod.lifeGreen);
            }
        }

        private bool HardcoreAndNotAllDeadForGood()
        {
            if (Player.difficulty != PlayerDifficultyID.Hardcore) {
                return false;
            }

            foreach (Player player in Main.player) {
                if (!player.active) {
                    continue;
                }

                if (player.difficulty == PlayerDifficultyID.Hardcore && !player.dead) {
                    return true;
                }
            }

            return false;
        }

        private bool AvoidMaxTimerAndWholeSecond()
            => timeSpentDead > 0 && Player.respawnTimer % 60 != 0;

        public override void UpdateDead()
        {
            if (ReviveModDisabled()) {
                return;
            }

            if ((respawnTimerPaused || CommonUtils.ActiveBossAlivePlayer() || HardcoreAndNotAllDeadForGood()) && AvoidMaxTimerAndWholeSecond()) {
                Player.respawnTimer++; // Undoes regular respawnTimer tick down
            }

            timeSpentDead++;
        }

        public override void OnRespawn()
        {
            if (ReviveModDisabled()) {
                return;
            }

            timeSpentDead = 0;
            ResetHardcorePlayer();
        }

        public override void PreUpdate()
        {
            if (ReviveModDisabled()) {
                return;
            }

            /* Done this way as aura despawning does not call OnKill */
            if (Main.myPlayer == Player.whoAmI && !auraActive && oldAuraActive) {
                Revive();
            }

            oldAuraActive = auraActive;
            auraActive = false;

            if (Main.myPlayer == Player.whoAmI && Player.difficulty == PlayerDifficultyID.Hardcore && Player.ghost && !oldGhost) {
                Player.KillMeForGood();
            }

            oldGhost = Player.ghost;
        }
    }
}
