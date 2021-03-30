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
    internal class StaticCharge : ItemBase.Item
    {
        BuffDef buffDef;
        public StaticCharge()
        {
            itemTemplate = new ItemTemplate
            {
                name = "Static Charge",
                tier = ItemTier.Tier2,
                internalName = "StaticCharge",
                pickupText = "Your critical strikes build up static charge to shock the next enemy to attack you.",
                descriptionText = "Gain <style=cIsDamage>5% critical chance</style>. <style=cIsDamage>Critical strikes</style> grant you a stack of <style=cIsDamage>static charge</style>. The next time you are attacked, <style=cIsUtility>shock</style> the attacker, dealing <style=cIsDamage>5% <style=cStack>(+5% per stack)</style> damage</style> for each stack of <style=cIsDamage>static charge</style>.",
                loreText = "I can feel a buzz in the air... Don't get to close to me... ",
            };
        }

        public override void Hook()
        {

            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;



            buffDef = ScriptableObject.CreateInstance<BuffDef>();

            buffDef.name = "Static Charge";
            buffDef.iconSprite = this.Icon;
            buffDef.isDebuff = false;
            buffDef.canStack = true;

            ItemBase.BuffProvider.AddBuff(buffDef);
        }

        BuffIndex buffIndex { get { return buffDef.buffIndex; } }

        void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self.inventory)
            {
                if(self.inventory.GetItemCount(this.itemIndex) > 0)
                {
                    self.crit += 5;
                }
            }
        }
        void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (damageInfo.attacker && damageInfo.attacker != self.gameObject)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (damageInfo.crit && attackerBody && attackerBody.master && attackerBody.master.inventory)
                {
                    if (attackerBody.master.inventory.GetItemCount(this.itemIndex) > 0)
                    {
                        attackerBody.AddBuff(this.buffIndex);
                    }
                }
                
                if(attackerBody && self.body.HasBuff(this.buffIndex))
                {
                    HealthComponent attackerHealthComponent = attackerBody.healthComponent;
                    if (attackerHealthComponent)
                    {
                        var zapDamageInfo = new DamageInfo()
                        {
                            attacker = self.body.gameObject,
                            crit = Util.CheckRoll(self.body.crit, self.body.master),
                            damage = self.body.damage * self.body.GetBuffCount(this.buffIndex) * 0.05f,
                            procCoefficient = 1,
                            damageType = DamageType.Shock5s
                        };
                        self.body.SetBuffCount(buffIndex, 0);
                        attackerHealthComponent.TakeDamage(zapDamageInfo);
                    }
                }
            }
            orig(self, damageInfo);
        }
    }
}
