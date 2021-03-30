using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using MonoMod.Cil;
using Mono.Cecil.Cil;

using RoR2;
using UnityEngine;

namespace MoreItems
{
    public class CritStorage : MonoBehaviour
    {
        public float crit = 1;
    }
    internal class SerratedSpoon : ItemBase.Item
    {
        public SerratedSpoon()
        {
            itemTemplate = new ItemTemplate
            {
                name = "Serrated Spoon",
                tier = ItemTier.Lunar,
                internalName = "SerratedSpoon",
                pickupText = "Your critical chance is always low, but your critical strikes get more damage from effects that add critical chance.",
                descriptionText = "Your <style=cIsDamage>critical chance</style> is <style=cIsHealth>fixed</style> to <style=cIsDamage>10%</style> <style=cStack>(halved per stack)</style>. <style=cIsDamage>Critical chance</style> effects grant you <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> <style=cIsDamage>critical damage</style> per 1% chance <style=cStack>(doubled per stack)</style>",
                loreText = "<style=cMono>//--AUTO-TRANSCRIPTION FROM KITCHEN 16C OF UES [Redacted] --//</style>\n\n MAN 1: Why could you POSSIBLY need a serrated spoon?\n\nMAN 2: Just in case.\n\nMAN 1: In case what?\n\nMan 2: You'll see...",
            };
        }
        public override void Hook()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
        }

        void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.inventory)
            {
                if (self.inventory.GetItemCount(itemDef) is int count && count > 0)
                {
                    CritStorage critStorage = self.gameObject.GetComponent<CritStorage>();
                    if (!critStorage)
                    {
                        critStorage = self.gameObject.AddComponent<CritStorage>();
                    }
                    critStorage.crit = self.crit;
                    self.crit = 10/Mathf.Pow(2, count - 1);
                }
            }
        }
        void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.crit)
            {
                if (damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>() is CharacterBody attackerBody && 
                    attackerBody.inventory && attackerBody.inventory.GetItemCount(itemDef) is int stacks && stacks > 0 &&
                    attackerBody.GetComponent<CritStorage>() is CritStorage critStorage)
                {
                    damageInfo.damage *= 1 + (stacks-1) + critStorage.crit * 0.1f * Mathf.Pow(2, stacks - 1);
                }
            }
            orig(self, damageInfo);
        }
    }

}
 