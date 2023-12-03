using Microsoft.Xna.Framework;
using ReviveMod.Source.Common.Players;
using ReviveMod.Source.Common.Projectiles;
using ReviveMod.Source.Common.Systems;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveMod.Source
{
    public class ReviveMod : Mod
    {
        public static readonly Color lifeGreen = new(52, 235, 73);

        internal enum MessageType : byte
        {
            AlivePlayerCheck,
            RevivePlayer,
            ReviveTeleport,
            ChangeReviveTime,
            SetRevive
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType type = (MessageType)reader.ReadByte();

            switch (type) {
                case MessageType.AlivePlayerCheck:
                    bool anyAlivePlayer = reader.ReadBoolean();

                    ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer = anyAlivePlayer;

                    break;

                case MessageType.RevivePlayer:
                    byte revivedWhoAmI = reader.ReadByte();

                    ReviveModPlayer revivedModPlayer = Main.player[revivedWhoAmI].GetModPlayer<ReviveModPlayer>();
                    revivedModPlayer.Revive(broadcast: false);

                    if (Main.netMode == NetmodeID.Server) {
                        revivedModPlayer.SendRevivePlayer(ignoreClient: whoAmI);
                    }

                    break;

                case MessageType.ReviveTeleport:
                    byte teleportingWhoAmI = reader.ReadByte();

                    ReviveModPlayer teleportingModPlayer = Main.player[teleportingWhoAmI].GetModPlayer<ReviveModPlayer>();
                    teleportingModPlayer.LocalTeleport();

                    if (Main.netMode == NetmodeID.Server) {
                        teleportingModPlayer.SendReviveTeleport(ignoreClient: whoAmI);
                    }

                    break;

                case MessageType.SetRevive:
                    bool state = reader.ReadBoolean();

                    ReviveModPlayer.SetRevive(state);

                    break;

                default:
                    Logger.WarnFormat("ReviveMod: Unknown message type: {0}", type);
                    break;
            }
        }
    }
}
