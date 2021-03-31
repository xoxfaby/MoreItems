using System;
using BepInEx;
using RoR2;
using UnityEngine;
using System.Reflection;
using BetterAPI;

namespace MoreItems
{
    [BepInDependency("com.xoxfaby.BetterAPI")]
    [BepInPlugin("com.xoxfaby.MoreItems", "MoreItems", "2.0.1.1")]
    public class MoreItems : BaseUnityPlugin
    {
        internal static AssetBundle bundle;

        static MoreItems()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.Unity.Assets.AssetBundles.itemasset"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                bundle.LoadAllAssets();
            }
        }
        public void Awake()
        {
            SerratedKnife.Add();
            StaticCharge.Add();
            CrackedOrb.Add();
            SerratedSpoon.Add();
            //itemProvider.AddItem(new LongerOSP());
        }
        internal static ItemDef AddItem(String name, ItemTier tier, String internalName, String pickupText, String descriptionText, String loreText)
        {
            var itemTemplate = new Items.ItemTemplate
            {
                name = name,
                tier = tier,
                internalName = internalName,
                prefab = MoreItems.bundle.LoadAsset<GameObject>($"Assets/Items/{internalName}/prefab.prefab"),
                icon = MoreItems.bundle.LoadAsset<Sprite>($"Assets/Items/{internalName}/icon.png"),
                pickupText = pickupText,
                descriptionText = descriptionText,
                loreText = loreText,
            };

            return Items.Add(itemTemplate);
        }
    }
}
