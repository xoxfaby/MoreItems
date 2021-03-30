using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal class SerratedKnife : ItemBase.Item
    {
        public SerratedKnife()
        {
            itemTemplate = new ItemTemplate
            {
                name = "Serrated Knife",
                tier = ItemTier.Tier1,
                internalName = "SerratedKnife",
                pickupText = "Your critical strikes do more damage.",
                descriptionText = "Your critial strikes deal <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> more damage.",
                loreText = "You might think it would be cruel to use a knife like this, but I think it's more humane, ending their suffering far more quickly...",
            };
        }

        public override void Hook()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
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
