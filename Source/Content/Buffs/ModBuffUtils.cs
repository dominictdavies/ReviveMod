using ReviveMod.Source.Content.Projectiles;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace ReviveMod.Source.Content.Buffs
{
    public class ModBuffUtils
    {
        public static void RemoveDebuffIfNotInAura(Player player, ref int buffIndex)
        {
            var reviveAuras = Main.projectile.Where(proj => proj.type == ModContent.ProjectileType<ReviveAura>()).ToArray();
            bool playerInReviveAura = reviveAuras.Any(proj => proj.Hitbox.Intersects(player.Hitbox));

            if (!playerInReviveAura) {
                player.DelBuff(buffIndex--);
            }
        }
    }
}
