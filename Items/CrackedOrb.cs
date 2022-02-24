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
            BetterAPI.ItemDisplays.CharacterItemDisplayRuleSet rules = new BetterAPI.ItemDisplays.CharacterItemDisplayRuleSet();
            rules.AddDefaultRule(new ItemDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    childName = "Head",
                    localPos = new Vector3(0.45f, 0.45f, 0f),
                    localAngles = new Vector3(0f, -40f, 0f),
                    localScale = new Vector3(0.2f, 0.2f, 0.2f),
                    followerPrefab = MoreItemsPlugin.bundle.LoadAsset<GameObject>($"Assets/Items/crackedorb/prefab.prefab"),
                }
            );

            itemDef = MoreItemsPlugin.AddItem(
                "Cracked Orb",
                 ItemTier.Lunar,
                "CrackedOrb",
                "You have an extra chance to duplicate your attacks, <color=#FF7F7F>but an equal chance to duplicate attacks that hit you.</color>",
                "You have a <style=cIsDamage>30%</style> <style=cStack>(+30% per stack)</style> chance to duplicate your attacks, <color=#FF7F7F>but an equal chance to duplicate attacks that hit you.</color>",
                "<style=cMono>NO DATA FOUND</style>",
                rules
            );
        }

        internal static int rollAttackCount(CharacterBody characterBody, bool onSelf = false)
        {
            if (characterBody.inventory)
            {
                var stacks = characterBody.inventory.GetItemCount(itemDef);
                var chance = 0.3f * stacks;
                var flat_chance = (int)chance;
                return flat_chance + Convert.ToInt32(RoR2.Util.CheckRoll((chance - flat_chance) * 100, onSelf ? -characterBody.master.luck : characterBody.master.luck));
            }
            return 0;
        }
        public static void Add()
        { 
            MoreItemsPlugin.Hooks.Add<RoR2.BulletAttack>( "Fire", BulletAttack_Fire );
            MoreItemsPlugin.Hooks.Add<RoR2.Projectile.ProjectileManager, RoR2.Projectile.FireProjectileInfo>( "FireProjectile", ProjectileManager_FireProjectile_FireProjectileInfo );
            MoreItemsPlugin.Hooks.Add<RoR2.Orbs.OrbManager, RoR2.Orbs.Orb>( "AddOrb", OrbManager_AddOrb );
            MoreItemsPlugin.Hooks.Add<RoR2.Orbs.LightningOrb>( "OnArrival", LightningOrb_OnArrival );
            MoreItemsPlugin.Hooks.Add<RoR2.OverlapAttack, List<HurtBox>, bool>( "Fire", OverlapAttack_Fire );
            MoreItemsPlugin.Hooks.Add<RoR2.OverlapAttack>( "ResetIgnoredHealthComponents", OverlapAttack_ResetIgnoredHealthComponents );
            MoreItemsPlugin.Hooks.Add<RoR2.HealthComponent, DamageInfo>( "TakeDamage", HealthComponent_TakeDamage );
            MoreItemsPlugin.Hooks.Add<EntityStates.Merc.Evis>( "FixedUpdate", Evis_FixedUpdate);
            MoreItemsPlugin.Hooks.Add<RoR2.Projectile.ProjectileSimple>( "Awake", ProjectileSimple_Awake );

            MoreItemsPlugin.onAwake += MoreItemsPlugin_onAwake;
        }

        private static void MoreItemsPlugin_onAwake()
        {
            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(30, i, Physics.GetIgnoreLayerCollision(8, i));
            }
            Physics.IgnoreLayerCollision(30, 30, true);
        }

        private static void ProjectileSimple_Awake(Action<RoR2.Projectile.ProjectileSimple> orig, RoR2.Projectile.ProjectileSimple self)
        {
            orig(self);
            if (self.gameObject.name == "MageIceBombProjectile(Clone)")
            {
                self.gameObject.layer = 30;
            }
        }

        static private void HealthComponent_TakeDamage(Action<RoR2.HealthComponent, DamageInfo> orig, HealthComponent self, DamageInfo damageInfo)
        {
            for (int i = 0; i < rollAttackCount(self.body, true); i++)
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

        static private void OverlapAttack_ResetIgnoredHealthComponents(Action<RoR2.OverlapAttack> orig, OverlapAttack self)
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

        static private bool OverlapAttack_Fire(Func<RoR2.OverlapAttack, List<HurtBox>, bool> orig, OverlapAttack self, List<HurtBox> hitResults)
        {
            if (self.attacker && self.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
            {
                for (int i = 0; i < rollAttackCount(characterBody); i++)
                {
                    if (!childAttacks.TryGetValue(self, out List<OverlapAttack> attacks))
                    {
                        attacks = new List<OverlapAttack>();
                        childAttacks.Add(self, attacks);
                    }
                    if (i >= attacks.Count)
                    {
                        var overlapAttack = new OverlapAttack
                        {
                            attacker = self.attacker,
                            attackerFiltering = self.attackerFiltering,
                            damage = self.damage,
                            damageColorIndex = self.damageColorIndex,
                            damageType = self.damageType,
                            forceVector = self.forceVector,
                            hitBoxGroup = self.hitBoxGroup,
                            hitEffectPrefab = self.hitEffectPrefab,
                            inflictor = self.inflictor,
                            isCrit = self.isCrit,
                            procChainMask = self.procChainMask,
                            procCoefficient = self.procCoefficient,
                            teamIndex = self.teamIndex
                        };
                        attacks.Add(overlapAttack);
                    }
                    orig(attacks[i], hitResults);
                }
            }
            return orig(self, hitResults);
        }

        static private void OrbManager_AddOrb(Action<RoR2.Orbs.OrbManager, RoR2.Orbs.Orb> orig, RoR2.Orbs.OrbManager self, RoR2.Orbs.Orb orb)
        {
            if (orb is RoR2.Orbs.LightningOrb lightningOrb && lightningOrb.attacker)
            {
                if (lightningOrb.attacker.GetComponent<CharacterBody>() is CharacterBody characterBody)
                {
                    if (!bouncedOrbs.Contains(lightningOrb))
                    {
                        for (int i = 0; i < rollAttackCount(characterBody) ; i++)
                        {
                            LightningOrb lightningOrbCopy = new LightningOrb
                            {
                                search = new BullseyeSearch(),
                                origin = lightningOrb.origin,
                                target = lightningOrb.target,
                                attacker = lightningOrb.attacker,
                                inflictor = lightningOrb.inflictor,
                                teamIndex = lightningOrb.teamIndex,
                                damageValue = lightningOrb.damageValue,
                                bouncesRemaining = lightningOrb.bouncesRemaining,
                                isCrit = lightningOrb.isCrit,
                                bouncedObjects = lightningOrb.bouncedObjects != null ? new List<HealthComponent>(lightningOrb.bouncedObjects) : new List<HealthComponent>(),
                                lightningType = lightningOrb.lightningType,
                                procChainMask = lightningOrb.procChainMask,
                                procCoefficient = lightningOrb.procCoefficient,
                                damageColorIndex = lightningOrb.damageColorIndex,
                                damageCoefficientPerBounce = lightningOrb.damageCoefficientPerBounce,
                                speed = lightningOrb.speed,
                                range = lightningOrb.range,
                                damageType = lightningOrb.damageType,
                                failedToKill = lightningOrb.failedToKill
                            };
                            orig(self, lightningOrbCopy);
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

        static private void ProjectileManager_FireProjectile_FireProjectileInfo(Action<RoR2.Projectile.ProjectileManager, RoR2.Projectile.FireProjectileInfo> orig, RoR2.Projectile.ProjectileManager self, RoR2.Projectile.FireProjectileInfo fireProjectileInfo)
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

        static private void BulletAttack_Fire(Action<RoR2.BulletAttack> orig, BulletAttack self)
        {
            if (self.owner)
            {
                if (self.owner.GetComponent<CharacterBody>() is CharacterBody characterBody && characterBody.inventory)
                {
                    if (rollAttackCount(characterBody) is int count && count > 0)
                    {
                        self.maxSpread = Mathf.Max(self.maxSpread*count*0.75f,2 * count);
                        self.bulletCount *= (uint)count + 1;
                    }
                }
            }
            orig(self);
        }
    }

}
