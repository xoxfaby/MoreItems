using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

using MonoMod.Cil;
using Mono.Cecil.Cil;

using BetterAPI;

using RoR2;
using UnityEngine;
using RoR2.Orbs;

namespace MoreItems
{
    internal static class CrackedOrb
    {
        static ConditionalWeakTable<OverlapAttack, List<OverlapAttack>> childAttacks = new ConditionalWeakTable<OverlapAttack, List<OverlapAttack>>();
        static List<LightningOrb> bouncedOrbs = new List<LightningOrb>();
        static ItemDef itemDef;
        static CrackedOrb()
        {
            BetterAPI.Items.CharacterItemDisplayRuleSet rules = new BetterAPI.Items.CharacterItemDisplayRuleSet();
            rules.AddDefaultRule(new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.45f, 0.45f, 0f),
                    localAngles = new Vector3(0f, -40f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f),
                    followerPrefab = MoreItems.bundle.LoadAsset<GameObject>($"Assets/Items/crackedorb/prefab.prefab"),
                }
            );

            Vector3 generalScale = new Vector3(1, 1, 1);
            itemDef = MoreItems.AddItem(
                "Cracked Orb",
                 ItemTier.Lunar,
                "CrackedOrb",
                "You have an extra chance to duplicate your attacks, <color=#FF7F7F>but an equal chance to duplicate attacks that hit you.</color>",
                "You have a <style=cIsDamage>30%</style> <style=cStack>(+30% per stack)</style> chance to duplicate your attacks, <color=#FF7F7F>but an equal chance to duplicate attacks that hit you.</color>",
                "<style=cMono>NO DATA FOUND</style>",
                rules
            );
        }

        internal static int rollAttackCount(CharacterBody characterBody)
        {
            if (characterBody.inventory)
            {
                var stacks = characterBody.inventory.GetItemCount(itemDef);
                var chance = 0.3f * stacks;
                var flat_chance = (int)chance;
                return flat_chance + Convert.ToInt32(RoR2.Util.CheckRoll((chance - flat_chance) * 100, characterBody.master));
            }
            return 0;
        }
        public static void Add()
        { 
            On.RoR2.BulletAttack.Fire += BulletAttack_Fire;
            On.RoR2.Projectile.ProjectileManager.FireProjectile_FireProjectileInfo += ProjectileManager_FireProjectile_FireProjectileInfo;
            On.RoR2.Orbs.OrbManager.AddOrb += OrbManager_AddOrb;
            IL.RoR2.Orbs.LightningOrb.OnArrival += LightningOrb_OnArrival;
            On.RoR2.OverlapAttack.Fire += OverlapAttack_Fire;
            On.RoR2.OverlapAttack.ResetIgnoredHealthComponents += OverlapAttack_ResetIgnoredHealthComponents;
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            IL.EntityStates.Merc.Evis.FixedUpdate += Evis_FixedUpdate;
            On.RoR2.Projectile.ProjectileSimple.Awake += ProjectileSimple_Awake;
        }

        private static void ProjectileSimple_Awake(On.RoR2.Projectile.ProjectileSimple.orig_Awake orig, RoR2.Projectile.ProjectileSimple self)
        {
            orig(self);
            if (self.gameObject.name == "MageIceBombProjectile(Clone)")
            {
                self.gameObject.layer = 21;
            }
        }

        static private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            for (int i = 0; i < rollAttackCount(self.body); i++)
            {
                orig(self, damageInfo);
            }
            orig(self, damageInfo);
        }


        static private void Evis_FixedUpdate(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdcR4(1f),
                x => x.MatchLdsfld<EntityStates.Merc.Evis>("damageFrequency"),
                x => x.MatchDiv());
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<EntityStates.Merc.Evis, float>>((self) =>
            { 
                if (self.characterBody)
                {
                    return 1f / (rollAttackCount(self.characterBody) + 1);
                }
                return 1f;
            });
        }

        static private void LightningOrb_OnArrival(ILContext il)
        {
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchNewobj<LightningOrb>());
            c.Index++;
            c.Emit(OpCodes.Dup);
            c.EmitDelegate<Action<LightningOrb>>((orb) => {
                bouncedOrbs.Add(orb);
            });
        }

        static private void OverlapAttack_ResetIgnoredHealthComponents(On.RoR2.OverlapAttack.orig_ResetIgnoredHealthComponents orig, OverlapAttack self)
        {

            if (childAttacks.TryGetValue(self, out var attacks))
            {
                foreach (var attack in attacks)
                {
                    orig(attack);
                }
            }
            orig(self);
        }

        static private bool OverlapAttack_Fire(On.RoR2.OverlapAttack.orig_Fire orig, OverlapAttack self, List<HurtBox> hitResults)
        {
            if (self.attacker && self.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
            {
                for (int i = 0; i < rollAttackCount(characterBody); i++)
                {
                    List<OverlapAttack> attacks;
                    if (!childAttacks.TryGetValue(self, out attacks))
                    {
                        attacks = new List<OverlapAttack>();
                        childAttacks.Add(self, attacks);
                    }
                    if (i >= attacks.Count)
                    {
                        OverlapAttack overlapAttack = new OverlapAttack();
                        overlapAttack.attacker = self.attacker;
                        overlapAttack.attackerFiltering = self.attackerFiltering;
                        overlapAttack.damage = self.damage;
                        overlapAttack.damageColorIndex = self.damageColorIndex;
                        overlapAttack.damageType = self.damageType;
                        overlapAttack.forceVector = self.forceVector;
                        overlapAttack.hitBoxGroup = self.hitBoxGroup;
                        overlapAttack.hitEffectPrefab = self.hitEffectPrefab;
                        overlapAttack.inflictor = self.inflictor;
                        overlapAttack.isCrit = self.isCrit;
                        overlapAttack.procChainMask = self.procChainMask;
                        overlapAttack.procCoefficient = self.procCoefficient;
                        overlapAttack.teamIndex = self.teamIndex;
                        attacks.Add(overlapAttack);
                    }
                    orig(attacks[i], hitResults);
                }
            }
            return orig(self, hitResults);
        }

        static private void OrbManager_AddOrb(On.RoR2.Orbs.OrbManager.orig_AddOrb orig, RoR2.Orbs.OrbManager self, RoR2.Orbs.Orb orb)
        {
            if (orb is RoR2.Orbs.LightningOrb lightningOrb && lightningOrb.attacker)
            {
                if (lightningOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
                {
                    if (!bouncedOrbs.Contains(lightningOrb))
                    {
                        for (int i = 0; i < rollAttackCount(characterBody) ; i++)
                        {
                            LightningOrb lightningOrbCopy = new LightningOrb();
                            lightningOrbCopy.search = new BullseyeSearch();
                            lightningOrbCopy.origin = lightningOrb.origin;
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
            else if (orb is RoR2.Orbs.GenericDamageOrb genericDamageOrb && genericDamageOrb.attacker)
            {
                if (genericDamageOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
                {
                    for (int i = 0; i < rollAttackCount(characterBody); i++)
                    {
                        orig(self, orb);
                    }
                }

            } 
            else if (orb is RoR2.Orbs.DamageOrb damageOrb && damageOrb.attacker)
            {
                if (damageOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
                {
                    for (int i = 0; i < rollAttackCount(characterBody); i++)
                    {
                        orig(self, orb);
                    }
                }

            } 
            else if (orb is RoR2.Orbs.DevilOrb devilOrb && devilOrb.attacker)
            {
                if (devilOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    for (int i = 0; i < rollAttackCount(characterBody); i++)
                    {
                        orig(self, orb);
                    }
                }

            }
            orig(self, orb);
        }

        static private void ProjectileManager_FireProjectile_FireProjectileInfo(On.RoR2.Projectile.ProjectileManager.orig_FireProjectile_FireProjectileInfo orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo)
        {
            if (fireProjectileInfo.owner && fireProjectileInfo.owner.GetComponent<CharacterBody>() is CharacterBody characterBody && rollAttackCount(characterBody) is int count && count > 0)
            {
                var oldRotation = fireProjectileInfo.rotation;
                Vector3 axis = Vector3.Cross(Vector3.up, fireProjectileInfo.rotation * Vector3.forward);
                float x = UnityEngine.Random.Range(0f, 2 * count);
                float z = UnityEngine.Random.Range(0f, 360f);
                Vector3 vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                float y = vector.y;
                vector.y = 0f;
                float angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f);
                float angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f;
                fireProjectileInfo.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle, Vector3.up) * (Quaternion.AngleAxis(angle2, axis) * fireProjectileInfo.rotation * Vector3.forward));
                for (int i = 0; i < count; i++)
                {
                    orig(self, fireProjectileInfo);
                    x = UnityEngine.Random.Range(0f, 2 * count);
                    z = UnityEngine.Random.Range(0f, 360f);
                    vector = Quaternion.Euler(0f, 0f, z) * (Quaternion.Euler(x, 0f, 0f) * Vector3.forward);
                    y = vector.y;
                    vector.y = 0f;
                    angle = (Mathf.Atan2(vector.z, vector.x) * 57.29578f - 90f);
                    angle2 = Mathf.Atan2(y, vector.magnitude) * 57.29578f;
                    fireProjectileInfo.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(angle, Vector3.up) * (Quaternion.AngleAxis(angle2, axis) * oldRotation * Vector3.forward));
                }

            }
            orig(self, fireProjectileInfo);
        }

        static private void BulletAttack_Fire(On.RoR2.BulletAttack.orig_Fire orig, BulletAttack self)
        {
            if (self.owner)
            {
                if (self.owner.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (rollAttackCount(characterBody) is int count && count > 0)
                    {
                        self.maxSpread = Mathf.Max(self.maxSpread*count*0.75f,2 * count);
                        self.bulletCount += (uint)count;
                    }
                }
            }
            orig(self);
        }
    }

}
