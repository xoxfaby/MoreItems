using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using MonoMod.Cil;
using Mono.Cecil.Cil;

using R2API;
using RoR2;
using UnityEngine;
using RoR2.Orbs;

namespace MoreItems
{
    internal class RangeIncrease : ItemBase
    {
        List<LightningOrb> bouncedOrbs = new List<LightningOrb>();
        internal RangeIncrease()
        {

            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.Orbs.OrbManager.AddOrb += OrbManager_AddOrb;
            IL.RoR2.Orbs.LightningOrb.OnArrival += LightningOrb_OnArrival;

            On.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            IL.RoR2.OverlapAttack.Fire += OverlapAttack_FireIL;

            var itemTemplate = new ItemTemplate();
            itemTemplate.name = "Projectile Spam";
            itemTemplate.tier = ItemTier.Lunar;
            itemTemplate.internalName = "PROJECTILE_SPAM";
            itemTemplate.pickupText = "Bullets go brrrrrrrrrrrrrrr";
            itemTemplate.descriptionText = "Bullets go brrrrrrrrrrrrrrr";
            itemTemplate.loreText = "Bullets go brrrrrrrrrrrrrrr";

            Init(itemTemplate);
        }

        private void LightningOrb_OnArrival(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchNewobj<LightningOrb>());
            c.Index++;
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<LightningOrb>>((orb) => {
                bouncedOrbs.Add(orb);
            });
        }


        private void OverlapAttack_FireIL(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdcR4(0.5f));
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<OverlapAttack, float>>((self) =>
            {
                if (self.attacker)
                {
                    if (self.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                    {
                        if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count && count > 0)
                        {
                            return (count + 1) * 0.5f;
                        }
                    }
                }
                return 0.5f;
            });
        }



        private bool OverlapAttack_Fire(On.RoR2.OverlapAttack.orig_Fire orig, OverlapAttack self, List<HealthComponent> hitResults)
        {
            if (self.attacker)
            {
                if (self.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count && count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            orig(self, hitResults);
                        }
                    }
                }
            }
            return orig(self, hitResults);
        }

        private void OrbManager_AddOrb(On.RoR2.Orbs.OrbManager.orig_AddOrb orig, RoR2.Orbs.OrbManager self, RoR2.Orbs.Orb orb)
        {
            MoreItems.print(bouncedOrbs.Count);
            if (orb is RoR2.Orbs.LightningOrb lightningOrb && lightningOrb.attacker)
            {
                if (lightningOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count)
                    {
                        if (!bouncedOrbs.Contains(lightningOrb))
                        {
                            for (int i = 0; i < count; i++)
                            {
                                LightningOrb lightningOrbCopy = new LightningOrb();
                                lightningOrbCopy.search = new BullseyeSearch();
                                lightningOrbCopy.origin = lightningOrb.target.transform.position;
                                lightningOrbCopy.target = lightningOrb.target;
                                lightningOrbCopy.attacker = lightningOrb.attacker;
                                lightningOrbCopy.inflictor = lightningOrb.inflictor;
                                lightningOrbCopy.teamIndex = lightningOrb.teamIndex;
                                lightningOrbCopy.damageValue = lightningOrb.damageValue;
                                lightningOrbCopy.bouncesRemaining = lightningOrb.bouncesRemaining;
                                lightningOrbCopy.isCrit = lightningOrb.isCrit;
                                lightningOrbCopy.bouncedObjects = new List<HealthComponent>(lightningOrb.bouncedObjects);
                                lightningOrbCopy.lightningType = lightningOrb.lightningType;
                                lightningOrbCopy.procChainMask = lightningOrb.procChainMask;
                                lightningOrbCopy.procCoefficient = lightningOrb.procCoefficient;
                                lightningOrbCopy.damageColorIndex = lightningOrb.damageColorIndex;
                                lightningOrbCopy.damageCoefficientPerBounce = lightningOrb.damageCoefficientPerBounce;
                                lightningOrbCopy.speed = lightningOrb.speed;
                                lightningOrbCopy.range = lightningOrb.range;
                                lightningOrbCopy.damageType = lightningOrb.damageType;
                                lightningOrbCopy.failedToKill = lightningOrb.failedToKill;
                                orig(self,lightningOrbCopy);
                            }
                        }
                        else
                        {
                            bouncedOrbs.Remove(lightningOrb);
                        }
                    }
                }

            }else if (orb is RoR2.Orbs.GenericDamageOrb genericDamageOrb && genericDamageOrb.attacker)
            {
                if (genericDamageOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            orig(self, orb);
                        }
                    }
                }

            } else if (orb is RoR2.Orbs.DamageOrb damageOrb && damageOrb.attacker)
            {
                if (damageOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            orig(self, orb);
                        }
                    }
                }

            } else if (orb is RoR2.Orbs.DevilOrb devilOrb && devilOrb.attacker)
            {
                if (devilOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            orig(self, orb);
                        }
                    }
                }

            }
            orig(self, orb);
        }

        private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.owner)
            {
                if (fireProjectileInfo.owner.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count && count > 0)
                    {
                        var oldRotation = fireProjectileInfo.rotation;
                        fireProjectileInfo.rotation = Quaternion.RotateTowards(oldRotation, UnityEngine.Random.rotation, (float)Math.Pow(2, count));
                        fireProjectileInfo.damage /= count + 1;
                        for(int i = 0; i < count; i++)
                        {
                            orig(self, fireProjectileInfo);
                            fireProjectileInfo.rotation = Quaternion.RotateTowards(oldRotation, UnityEngine.Random.rotation, (float)Math.Pow(2, count));
                        }
                    }
                }
            }
            orig(self, fireProjectileInfo);
        }

        private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self)
        {
            if (self.owner)
            {
                if (self.owner.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (characterBody.inventory.GetItemCount(this.ItemIndex) is int count && count > 0)
                    {
                        self.minSpread = self.maxSpread;
                        self.maxSpread *= 1 + count;
                        self.bulletCount *= (uint)( 1 + count);
                        self.procCoefficient /= 1 + count;
                        self.damage /= 1 + count;
                    }
                }
            }
            orig(self);
        }
    }

}
