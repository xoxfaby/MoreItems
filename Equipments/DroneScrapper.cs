﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using MonoMod.Cil;
using Mono.Cecil.Cil;

using RoR2;
using RoR2.DirectionalSearch;
using UnityEngine;
using System.Linq;

namespace MoreItems
{
    public static class DroneScrapper
    {
        public static readonly Dictionary<String, ItemDef> DroneScrapMap = new Dictionary<String, ItemDef>();
        public static EquipmentDef equipmentDef;
        static DroneScrapper()
        {
            

            equipmentDef = BetterAPI.Equipments.Add(new BetterAPI.Equipments.EquipmentTemplate()
            {
                internalName = "DroneScrapper",
                name = "Drone Scrapper",
                pickupText = "Disassemble a drone or turret to recieve scrap.",
                descriptionText = "<style=cIsUtility>Disassemble</style> a drone to recieve <style=cIsUtility>scrap</style>. <style=cIsUtility>More advanced drones create higher tiers of scrap</style>.",
                loreText = "\"How am I supposed to afford all of these drones?\"\n\n - Some broke guy",
                canDrop = true,
                cooldown = 65,
                prefab = MoreItemsPlugin.GeneratePlaceholderPrefab("Drone\nScrapper")
            });
        }

        public static void Add()
        {
            MoreItemsPlugin.Hooks.Add<RoR2.EquipmentSlot>("PerformEquipmentAction", EquipmentSlot_PerformEquipmentAction);
            MoreItemsPlugin.Hooks.Add<RoR2.EquipmentSlot>("UpdateTargets", EquipmentSlot_UpdateTargets);
            MoreItemsPlugin.onStart += MoreItemsPlugin_onStart;
            RoR2.ItemCatalog.availability.CallWhenAvailable(() =>
            {
                DroneScrapMap["TURRET1_CONTEXT"] = RoR2.RoR2Content.Items.ScrapWhite;
                DroneScrapMap["DRONE_GUNNER_CONTEXT"] = RoR2.RoR2Content.Items.ScrapWhite;
                DroneScrapMap["DRONE_HEALING_CONTEXT"] = RoR2.RoR2Content.Items.ScrapWhite;
                DroneScrapMap["DRONE_MISSILE_CONTEXT"] = RoR2.RoR2Content.Items.ScrapGreen;
                DroneScrapMap["FLAMEDRONE_CONTEXT"] = RoR2.RoR2Content.Items.ScrapGreen;
                DroneScrapMap["EQUIPMENTDRONE_CONTEXT"] = RoR2.RoR2Content.Items.ScrapGreen;
                DroneScrapMap["EMERGENCYDRONE_CONTEXT"] = RoR2.RoR2Content.Items.ScrapGreen;
                DroneScrapMap["DRONE_MEGA_CONTEXT"] = RoR2.RoR2Content.Items.ScrapRed;
            });
        }

        private static void MoreItemsPlugin_onStart()
        {
            equipmentDef.pickupIconSprite = MoreItemsPlugin.GeneratePlaceholderSprite("Drone\nScrapper");
        }


        public struct DroneSearchSelector : IGenericWorldSearchSelector<PurchaseInteraction>
        {
            public Transform GetTransform(PurchaseInteraction source)
            {
                return source.transform;
            }

            public GameObject GetRootObject(PurchaseInteraction source)
            {
                return source.gameObject;
            }
        }

        public struct DroneSearchFilter : IGenericDirectionalSearchFilter<PurchaseInteraction>
        {
            internal List<String> interactionFilters;
            public bool PassesFilter(PurchaseInteraction purchaseInteraction)
            {
                if (interactionFilters != null)
                {
                    return interactionFilters.Contains(purchaseInteraction.contextToken);
                }
                return true;
            }
        }

        internal class DroneSearch : BaseDirectionalSearch<PurchaseInteraction, DroneSearchSelector, DroneSearchFilter>
        {
            internal DroneSearch() : base(default(DroneSearchSelector), default(DroneSearchFilter))
            {
            }

            public DroneSearch(List<String> filters) : base(default(DroneSearchSelector), default(DroneSearchFilter))
            {
                this.candidateFilter.interactionFilters = filters;
            }

            public DroneSearch(DroneSearchSelector selector, DroneSearchFilter candidateFilter) : base(selector, candidateFilter)
            {
            }
        }

        static System.Runtime.CompilerServices.ConditionalWeakTable<EquipmentSlot, DroneSearch> table = new System.Runtime.CompilerServices.ConditionalWeakTable<EquipmentSlot, DroneSearch>();

        static void EquipmentSlot_UpdateTargets(Action<EquipmentSlot, EquipmentIndex, bool> orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
        {
            orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            if (targetingEquipmentIndex == DroneScrapper.equipmentDef.equipmentIndex)
            {
                getTarget(self);
            }
        }

        static PurchaseInteraction getTarget(EquipmentSlot slot)
        {
            var aimRay = slot.GetAimRay();
            DroneSearch search;
            if (!table.TryGetValue(slot, out search))
            {
                search = new DroneSearch(DroneScrapMap.Keys.ToList());
            }
            float num;
            aimRay = CameraRigController.ModifyAimRayIfApplicable(aimRay, slot.gameObject, out num);
            search.searchOrigin = aimRay.origin;
            search.searchDirection = aimRay.direction;
            search.minAngleFilter = 0f;
            search.maxAngleFilter = 10f;
            search.minDistanceFilter = 0f;
            search.maxDistanceFilter = 30f + num;
            search.filterByDistinctEntity = false;
            search.filterByLoS = false;
            search.sortMode = SortMode.DistanceAndAngle;
            var result = search.SearchCandidatesForSingleTarget<List<PurchaseInteraction>>(InstanceTracker.GetInstancesList<PurchaseInteraction>());
            slot.currentTarget = new EquipmentSlot.UserTargetInfo
            {
                transformToIndicateAt = result ? result.transform : null,
                rootObject = result ? result.gameObject : null
            };
            slot.targetIndicator.visualizerPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/RecyclerIndicator");
            slot.targetIndicator.active = result;
            slot.targetIndicator.targetTransform = (result ? slot.currentTarget.transformToIndicateAt : null);
            return result;
        }

        static bool EquipmentSlot_PerformEquipmentAction(Func<RoR2.EquipmentSlot, RoR2.EquipmentDef, bool> orig, EquipmentSlot self, RoR2.EquipmentDef equipmentDef)
        {
            if (DroneScrapper.equipmentDef == equipmentDef)
            {
                self.subcooldownTimer = 0.2f;
                self.InvalidateCurrentTarget();
                var target = getTarget(self);
                if (target != null)
                {
                    PickupDropletController.CreatePickupDroplet(
                        PickupCatalog.FindPickupIndex(DroneScrapMap[target.contextToken].itemIndex),
                        target.gameObject.transform.position + new Vector3(0f, 1.25f, 0f),
                        new Vector3(UnityEngine.Random.Range(-4f, 4f), 20f, UnityEngine.Random.Range(-4f, 4f))
                    );
                    UnityEngine.GameObject.Destroy(target.gameObject);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            return orig(self, equipmentDef);
        }
    }
}
