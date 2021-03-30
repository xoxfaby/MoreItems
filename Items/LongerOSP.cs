using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace MoreItems
{
    internal class LongerOSP : ItemBase.Item
    {
        BuffIndex buffIndex;
        internal LongerOSP()
        {
            On.RoR2.HealthComponent.TriggerOneShotProtection += HealthComponent_TriggerOneShotProtection;
            On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;

            var itemTemplate = new ItemTemplate();
            itemTemplate.name = "Longer OSP";
            itemTemplate.tier = ItemTier.Tier1;
            itemTemplate.internalName = "LONGER_OSP";
            itemTemplate.pickupText = "LONGER_OSP";
            itemTemplate.descriptionText = "LONGER_OSP";
            itemTemplate.loreText = "LONGER_OSP";

        }

        private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(this.buffIndex))
            {

            }
        }

        private void HealthComponent_TriggerOneShotProtection(On.RoR2.HealthComponent.orig_TriggerOneShotProtection orig, HealthComponent self)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.HealthComponent::TriggerOneShotProtection()' called on client");
                return;
            }
            if (self.body.inventory && self.body.inventory.GetItemCount(this.itemIndex) is int stacks && stacks > 0)
            {
                self.ospTimer = 1 * stacks;
            }
            else
            {
                orig(self);
            }
        }
    }
}
