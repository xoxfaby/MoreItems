using System;
using System.Collections.Generic;
using System.Text;

using R2API;
using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal abstract class ItemBase
    {
        internal GameObject Prefab;
        internal ItemIndex ItemIndex;

        internal string PrefabPath;
        internal string IconPath;

        protected void Init(ItemTemplate itemTemplate)
        {
            PrefabPath = $"{MoreItems.ModPrefix}Assets/{itemTemplate.internalName}.prefab";
            IconPath = $"{MoreItems.ModPrefix}Assets/{itemTemplate.internalName}.png";



                Prefab = MoreItems.bundle.LoadAsset<GameObject>($"Assets/{itemTemplate.internalName}.prefab");

            var itemDef = new ItemDef
            {
                name = itemTemplate.internalName,
                tier = itemTemplate.tier,
                pickupModelPath = PrefabPath,
                pickupIconPath = IconPath,
                nameToken = $"{itemTemplate.internalName}_NAME",
                pickupToken = $"{itemTemplate.internalName}_PICKUP",
                descriptionToken = $"{itemTemplate.internalName}_DESC",
                loreToken = $"{itemTemplate.internalName}_LORE",
                tags = itemTemplate.tags ?? new ItemTag[] { }
            };


            ItemIndex = ItemAPI.Add(new R2API.CustomItem(itemDef, itemTemplate.itemDisplayRules));

            R2API.LanguageAPI.Add(itemDef.nameToken, itemTemplate.name);
            R2API.LanguageAPI.Add(itemDef.pickupToken, itemTemplate.pickupText);
            R2API.LanguageAPI.Add(itemDef.descriptionToken, itemTemplate.descriptionText);
            R2API.LanguageAPI.Add(itemDef.loreToken, itemTemplate.loreText);
        }
        public struct ItemTemplate
        {
            public string internalName;
            public string name;
            public string pickupText;
            public string descriptionText;
            public string loreText;
            public ItemTier tier;
            public ItemTag[] tags;
            public ItemDisplayRule[] itemDisplayRules;
        }
    }
}
