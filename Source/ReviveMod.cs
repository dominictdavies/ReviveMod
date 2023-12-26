using Microsoft.Xna.Framework;
using ReviveMod.Common.Configs;
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

        public static bool Enabled => ModContent.GetInstance<ReviveModConfig>().Enabled && Main.netMode != NetmodeID.SinglePlayer;

        internal enum MessageType : byte
        {
            AlivePlayerCheck
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType type = (MessageType)reader.ReadByte();

            switch (type) {
                case MessageType.AlivePlayerCheck:
                    bool anyAlivePlayer = reader.ReadBoolean();

                    ModContent.GetInstance<ReviveModSystem>().anyAlivePlayer = anyAlivePlayer;

                    break;

                default:
                    Logger.WarnFormat("ReviveMod: Unknown message type: {0}", type);
                    break;
            }
        }
    }
}
