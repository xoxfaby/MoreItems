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
            PrefabPath = $"{MoreItems.ModPrefix}Assets/Items/{itemTemplate.internalName}/prefab.prefab";
            IconPath = $"{MoreItems.ModPrefix}Assets/Items/{itemTemplate.internalName}/icon.png";



            Prefab = MoreItems.bundle.LoadAsset<GameObject>($"Assets/Items/{itemTemplate.internalName}/prefab.prefab");

            var itemDef = new ItemDef
            {
                name = itemTemplate.internalName,
                tier = itemTemplate.tier,
                pickupModelPath = PrefabPath,
                pickupIconPath = IconPath,
                nameToken = $"ITEM_{itemTemplate.internalName.ToUpper()}_NAME",
                pickupToken = $"ITEM_{itemTemplate.internalName.ToUpper()}_PICKUP",
                descriptionToken = $"ITEM_{itemTemplate.internalName.ToUpper()}_DESC",
                loreToken = $"ITEM_{itemTemplate.internalName.ToUpper()}_LORE",
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
