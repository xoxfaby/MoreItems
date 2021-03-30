using System;
using BepInEx;
using RoR2;
using UnityEngine;
using System.Reflection;

namespace MoreItems
{
    [BepInDependency("com.xoxfaby.ItemBase")]
    [BepInPlugin("com.xoxfaby.MoreItems", "MoreItems", "2.0.0.1")]
    public class MoreItems : BaseUnityPlugin
    {
        public void Awake()
        {
            AssetBundle bundle;
            using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.Unity.Assets.AssetBundles.itemasset"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                bundle.LoadAllAssets();
            }
            var itemProvider = new ItemBase.ItemProvider(bundle);
            itemProvider.AddItem(new SerratedKnife());
            itemProvider.AddItem(new StaticCharge());
            itemProvider.AddItem(new CrackedOrb());
            itemProvider.AddItem(new SerratedSpoon());
            //itemProvider.AddItem(new LongerOSP());
        }
    }
}
