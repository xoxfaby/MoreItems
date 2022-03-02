using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using BetterAPI;
using RoR2;
using UnityEngine;

namespace MoreItems
{
    internal static class SerratedKnife
    {
        static ItemDef itemDef;
        static SerratedKnife()
        {
            itemDef = MoreItemsPlugin.AddItem(
                "Serrated Knife",
                ItemTier.Tier1,
                "SerratedKnife",
                "Your critical strikes do more damage.",
                "Your critial strikes deal <style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> more damage.",
                "You might think it would be cruel to use a knife like this, but I think it's more humane, ending their suffering far more quickly..."
            );
        }

        public static void Add()
        {
            BetterAPI.Stats.CriticalDamage.collectBonuses += CriticalDamage_collectBonuses;
        }

        private static void CriticalDamage_collectBonuses(CharacterBody characterBody, Stats.Stat.StatBonusArgs e)
        {
            if (characterBody.master && characterBody.master.inventory)
            {
                e.FlatBonuses.Add(0.1f * characterBody.master.inventory.GetItemCount(itemDef));
            }
        }
    }
}
