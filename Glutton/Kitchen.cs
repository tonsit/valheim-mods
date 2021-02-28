using HarmonyLib;
using System.Collections.Generic;
using BepInEx.Logging;
using System.Linq;
using System;
using UnityEngine;

namespace Glutton
{

    class ScoredFood : IComparable<ScoredFood>
    {
        public float score;

        public ItemDrop.ItemData food;

        public ScoredFood(ItemDrop.ItemData food)
        {
            this.food = food;
            score = GetFitnessScore(food);
        }

        int IComparable<ScoredFood>.CompareTo(ScoredFood food)
        {
            return Comparer<float>.Default.Compare(food.score, score);
        }

        static float GetFitnessScore(ItemDrop.ItemData food)
        {
            float score = food.m_shared.m_food * 50;
            score += food.m_shared.m_foodBurnTime * 1;
            score += food.m_shared.m_foodRegen * 500;
            score += food.m_shared.m_foodStamina * 50;
            return score;
        }
    }

    [HarmonyPatch]
    class Kitchen
    {
        static List<ItemDrop> all;

        static List<ScoredFood> sorted;

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ObjectDB), "Awake")]
        static List<ScoredFood> GetAllFoods()
        {
            all = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");
            sorted = new List<ScoredFood>();
            foreach (ItemDrop item in all)
            {
                if (item.m_itemData.m_shared.m_food > 0)
                {
                    string prefabName = item.GetPrefabName(item.gameObject.name);
                    GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(prefabName);
                    item.m_itemData.m_dropPrefab = itemPrefab;
                    Log($"Adding: {item.m_itemData.m_dropPrefab.name}", LogLevel.Debug);
                    sorted.Add(GetScoredFood(item.m_itemData));
                }
            }
            sorted.Sort();
            return sorted;
        }

        public static ItemDrop.ItemData GetBestFoodInGameExcept(List<ItemDrop.ItemData> activeFoodItems = default)
        {
            List<ScoredFood> activeFoods = ConvertItemsToScoredFood(activeFoodItems);
            List<ScoredFood> kitchenFoods = GetAllFoods();
            List<ScoredFood> sort = Remove(kitchenFoods, activeFoods);
            sort.Sort();
            return sort[0].food;
        }

        static List<ScoredFood> ConvertItemsToScoredFood(List<ItemDrop.ItemData> items)
        {
            List<ScoredFood> scored = new List<ScoredFood>();
            foreach (ItemDrop.ItemData item in items)
            {
                scored.Add(new ScoredFood(item));
            }
            return scored;
        }

        public static ItemDrop.ItemData GetBestFoodFromInventoryExcept(List<ItemDrop.ItemData> inventoryItems, List<ItemDrop.ItemData> activeFoodItems)
        {

            if (inventoryItems == default(List<ItemDrop.ItemData>))
            {
                return null;
            }
            List<ScoredFood> activeFoods = ConvertItemsToScoredFood(activeFoodItems);
            List<ScoredFood> inventoryFoods = ConvertItemsToScoredFood(inventoryItems);
            List<ScoredFood> sort = Remove(inventoryFoods, activeFoods);
            sort.Sort();
            if (sort.Count > 0)
            {
                return sort[0].food;
            }
            return null;
        }



        static List<ScoredFood> Remove(List<ScoredFood> inventory, List<ScoredFood> activeFoods)
        {
            List<ScoredFood> sort = new List<ScoredFood>();

            foreach (ScoredFood food in inventory)
            {
                if (!Has(activeFoods, food))
                {
                    sort.Add(food);
                }
            }
            return sort;
        }

        static bool Has(List<ScoredFood> activeFoods, ScoredFood food)
        {
            foreach (ScoredFood aFood in activeFoods)
            {
                if (aFood.food.m_shared.m_name == food.food.m_shared.m_name)
                    return true;
            }
            return false;
        }
        static ScoredFood GetScoredFood(ItemDrop.ItemData food)
        {
            return new ScoredFood(food);
        }

        private static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }
    }
}