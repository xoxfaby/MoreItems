using System;
using BepInEx;
using RoR2;
using UnityEngine;
using System.Reflection;
using BetterAPI;

namespace MoreItems
{
    [BepInDependency("com.xoxfaby.BetterAPI")]
    [BepInPlugin("com.xoxfaby.MoreItems", "MoreItems", "2.2.2")]
    public class MoreItemsPlugin : BetterUnityPlugin.BetterUnityPlugin<MoreItemsPlugin>
    {
        internal static AssetBundle bundle;
        internal static GameObject spriteRenderer;
        internal static RenderTexture renderTexture;
        internal static RoR2.ExpansionManagement.ExpansionDef expansion;
        public override BaseUnityPlugin typeReference => throw new NotImplementedException();

        static MoreItemsPlugin()
        {
            bundle = AssetBundle.LoadFromMemory(Properties.Resources.moreitems);
            spriteRenderer = bundle.LoadAsset<GameObject>($"Assets/Icons/SpriteGenerator.prefab");
            renderTexture = bundle.LoadAsset<RenderTexture>($"Assets/Icons/SpriteGeneratorRenderTexture.renderTexture");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            var expansionDef = ScriptableObject.CreateInstance<RoR2.ExpansionManagement.ExpansionDef>();
            expansionDef.nameToken = "MoreItems";
            expansionDef.descriptionToken = "Adds Items and Equipments from the 'MoreItems' mod to the game.";
            expansionDef.iconSprite = bundle.LoadAsset<Sprite>($"Assets/Icons/expansion_icon.png");
            expansionDef.disabledIconSprite = RoR2.LegacyResourcesAPI.Load<Sprite>("Textures/MiscIcons/texUnlockIcon");
            expansionDef.requiredEntitlement = null;
            expansion = expansionDef; 


            BetterAPI.ContentPacks.GetContentPackProvider().contentPack.expansionDefs.Add(new RoR2.ExpansionManagement.ExpansionDef[1]
            {
                expansionDef
            });

            SerratedKnife.Add();
            StaticCharge.Add();
            CrackedOrb.Add();
            SerratedSpoon.Add();
            DroneScrapper.Add();
            //itemProvider.AddItem(new LongerOSP());

        }

        internal static ItemDef AddItem(String name, ItemTier tier, String internalName, String pickupText, String descriptionText, String loreText, BetterAPI.ItemDisplays.CharacterItemDisplayRule[] characterItemDisplayRules = null)
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
                characterItemDisplayRules = characterItemDisplayRules,
            };
            var itemDef = Items.Add(itemTemplate);
            itemDef.requiredExpansion = expansion;
            return itemDef;
        }

        public static GameObject GeneratePlaceholderPrefab(string name)
        {
            var prefab = MoreItemsPlugin.bundle.LoadAsset<GameObject>($"Assets/Items/Placeholder.prefab");
            var textMeshes = prefab.GetComponentsInChildren<TMPro.TextMeshPro>();
            foreach (var textMesh in textMeshes)
            {
                textMesh.text = name;
            }
            return prefab;
        }

        public static Sprite GeneratePlaceholderSprite(string name)
        {
            var renderer = GameObject.Instantiate(spriteRenderer);
            renderer.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = name;
            var camera = renderer.GetComponentInChildren<Camera>();
            RenderTexture.active = renderTexture;
            camera.targetTexture = renderTexture;
            camera.Render();
            var rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();
            var sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            return sprite;
        }
    }
}
