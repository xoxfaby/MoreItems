using System;
using BepInEx;
using RoR2;
using UnityEngine;
using System.Reflection;
using BetterAPI;

namespace MoreItems
{
    [BepInDependency("com.xoxfaby.BetterAPI")]
    [BepInPlugin("com.xoxfaby.MoreItems", "MoreItems", "2.1.4.1")]
    public class MoreItemsPlugin : BetterUnityPlugin<MoreItemsPlugin>
    {
        internal static AssetBundle bundle;

        public override BaseUnityPlugin typeReference => throw new NotImplementedException();

        static MoreItemsPlugin()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MoreItems.Unity.Assets.AssetBundles.itemasset"))
            {
                bundle = AssetBundle.LoadFromStream(stream);
                bundle.LoadAllAssets();
            }
        }
        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(21, i, Physics.GetIgnoreLayerCollision(8, i));
            }
            Physics.IgnoreLayerCollision(21, 21, true);
            SerratedKnife.Add();
            StaticCharge.Add();
            CrackedOrb.Add();
            SerratedSpoon.Add();
            //itemProvider.AddItem(new LongerOSP());
        }

        internal static ItemDef AddItem(String name, ItemTier tier, String internalName, String pickupText, String descriptionText, String loreText, BetterAPI.Items.CharacterItemDisplayRule[] characterItemDisplayRules = null)
        {
            var itemTemplate = new Items.ItemTemplate
            {
                name = name,
                tier = tier,
                internalName = internalName,
                prefab = MoreItemsPlugin.bundle.LoadAsset<GameObject>($"Assets/Items/{internalName}/prefab.prefab"),
                icon = MoreItemsPlugin.bundle.LoadAsset<Sprite>($"Assets/Items/{internalName}/icon.png"),
                pickupText = pickupText,
                descriptionText = descriptionText,
                loreText = loreText,
                characterItemDisplayRules = characterItemDisplayRules
            };

            return Items.Add(itemTemplate);
        }
    }
}
