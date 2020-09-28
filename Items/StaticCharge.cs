using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using R2API;
using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal class StaticCharge : ItemBase
    {
        BuffIndex buffIndex;
        internal StaticCharge()
        {

            On.RoR2.HealthComponent.TakeDamage += hook_HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += hook_CharacterBody_RecalculateStats;

            var itemTemplate = new ItemTemplate();
            itemTemplate.name = "Static Charge";
            itemTemplate.tier = ItemTier.Tier2;
            itemTemplate.internalName = "STATIC_CHARGE";
            itemTemplate.pickupText = "Your critical strikes build up static charge to shock the next enemy to attack you.";
            itemTemplate.descriptionText = "Gain <style=cIsDamage>5% critical chance</style>. <style=cIsDamage>Critical strikes</style> grant you a stack of <style=cIsDamage>static charge</style>. The next time you are attacked, <style=cIsUtility>shock</style> the attacker, dealing <style=cIsDamage>5% <style=cStack>(+5% per stack)</style> damage</style> for each stack of <style=cIsDamage>static charge</style>.";
            itemTemplate.loreText = "I can feel a buzz in the air... Don't get to close to me... ";

            Init(itemTemplate);

            var buffDef = new BuffDef()
            {
                name = "Static Charge",
                iconPath = this.IconPath,
                isDebuff = false,
                canStack = true,
            };

            buffIndex = BuffAPI.Add(new CustomBuff(buffDef));
        }

        void hook_CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.inventory)
            {
                self.crit += self.inventory.GetItemCount(this.ItemIndex) > 0 ? 5f : 0 ;
            }
        }
        void hook_HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (damageInfo.crit && attackerBody && attackerBody.master && attackerBody.master.inventory)
                {
                    if (attackerBody.master.inventory.GetItemCount(this.ItemIndex) > 0)
                    {
                        attackerBody.AddBuff(this.buffIndex);
                    }
                }
                
                if(attackerBody && self.body.HasBuff(this.buffIndex))
                {
                    HealthComponent attackerHealthComponent = attackerBody.healthComponent;
                    if (attackerHealthComponent)
                    {
                        attackerHealthComponent.TakeDamage(new DamageInfo()
                        {
                            attacker = self.body.gameObject,
                            crit = Util.CheckRoll(self.body.crit, self.body.master),
                            damage = self.body.damage * self.body.GetBuffCount(this.buffIndex) * 0.05f,
                            procCoefficient = 1,
                            damageType = DamageType.Shock5s
                        });
                        self.body.SetBuffCount(buffIndex,0);
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}
