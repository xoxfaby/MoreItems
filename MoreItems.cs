using System;
using BepInEx;
using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine;
using System.Reflection;

namespace MoreItems
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.xoxfaby.MoreItems", "MoreItems", "1.1.2")]
    [R2APISubmoduleDependency(nameof(LanguageAPI), nameof(ItemAPI), nameof(ItemDropAPI), nameof(ResourcesAPI), nameof(BuffAPI))]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MoreItems : BaseUnityPlugin
    {
        public static AssetBundle bundle;
        public static string ModPrefix = "@MoreItems:";
        static MoreItems()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.itemasset"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                var provider = new AssetBundleResourcesProvider(ModPrefix.TrimEnd(':'), bundle);
                ResourcesAPI.AddProvider(provider);
            }
        }
        public void Awake()
        {

            new SerratedKnife();
            new StaticCharge();
            new CrackedOrb();
            new SerratedSpoon();
        }
    }
}
