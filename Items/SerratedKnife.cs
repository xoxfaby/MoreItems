using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using R2API;
using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal class SerratedKnife : ItemBase
    {
        internal SerratedKnife()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

            var itemTemplate = new ItemTemplate();
            itemTemplate.name = "Serrated Knife";
            itemTemplate.tier = ItemTier.Tier1;
            itemTemplate.internalName = "SerratedKnife";
            itemTemplate.pickupText = "Your critical strikes do more damage.";
            itemTemplate.descriptionText = "Your critial strikes deal <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> more damage.";
            itemTemplate.loreText = "You might think it would be cruel to use a knife like this, but I think it's more humane, ending their suffering far more quickly...";

            Init(itemTemplate);
        }

        void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.crit && damageInfo.attacker)
            {
                CharacterBody characterBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (characterBody && characterBody.master && characterBody.master.inventory)
                {
                    damageInfo.damage *= 0.10f * characterBody.master.inventory.GetItemCount(this.ItemIndex);
                }
            }
            orig(self, damageInfo);
        }
    }
}
