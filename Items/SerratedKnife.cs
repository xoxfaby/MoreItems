using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using BetterAPI;
using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal static class SerratedKnife
    {
        static ItemDef itemDef;
        static SerratedKnife()
        {
            itemDef = MoreItemsPlugin.AddItem(
                "Serrated Knife",
                ItemTier.Tier1,
                "SerratedKnife",
                "Your critical strikes do more damage.",
                "Your critial strikes deal <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> more damage.",
                "You might think it would be cruel to use a knife like this, but I think it's more humane, ending their suffering far more quickly..."
            );
        }

        public static void Add()
        {
            
            MoreItemsPlugin.Hooks.Add<RoR2.HealthComponent, DamageInfo>( "TakeDamage", HealthComponent_TakeDamage );
        }

        static void HealthComponent_TakeDamage(Action<RoR2.HealthComponent, DamageInfo> orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.crit && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody && attackerBody.master && attackerBody.master.inventory)
                {
                    damageInfo.damage *= (float) (1 + 0.1 * attackerBody.master.inventory.GetItemCount(itemDef));
                }
            }
            orig(self, damageInfo);
        }
    }
}
