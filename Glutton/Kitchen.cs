using HarmonyLib;
using System.Collections.Generic;
using BepInEx.Logging;
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
            if (GetConfigEatBestFoodsFirst()) {
                return Comparer<float>.Default.Compare(food.score, score);
            }
            return Comparer<float>.Default.Compare(score, food.score);
        }

        static float GetFitnessScore(ItemDrop.ItemData food)
        {
            return food.m_shared.m_food * GetConfigFoodHealthScoreWeight()
             + food.m_shared.m_foodBurnTime * GetConfigFoodBurnTimeScoreWeight()
             + food.m_shared.m_foodRegen * GetConfigFoodRegenScoreWeight()
             + food.m_shared.m_foodStamina * GetConfigFoodStaminaScoreWeight();
        }

        static float GetConfigFoodHealthScoreWeight()
        {
            return Glutton.GetConfigFoodHealthScoreWeight();
        }

        static float GetConfigFoodBurnTimeScoreWeight()
        {
            return Glutton.GetConfigFoodBurnTimeScoreWeight();
        }

        static float GetConfigFoodRegenScoreWeight()
        {
            return Glutton.GetConfigFoodRegenScoreWeight();
        }

        static float GetConfigFoodStaminaScoreWeight()
        {
            return Glutton.GetConfigFoodStaminaScoreWeight();
        }

        static bool GetConfigEatBestFoodsFirst()
        {
            return Glutton.GetConfigEatBestFoodsFirst();
        }
    }

    [HarmonyPatch]
    class Kitchen
    {
        static List<ItemDrop> all;

        static List<ScoredFood> sorted;

        public static ItemDrop.ItemData GetFoodFromKitchenExcept(List<ItemDrop.ItemData> activeFoodItems = default)
        {
            List<ScoredFood> activeFoods = ConvertItemsToScoredFood(activeFoodItems);
            List<ScoredFood> kitchenFoods = GetAllFoods();
            List<ScoredFood> sort = Remove(kitchenFoods, activeFoods);
            sort.Sort();
            if (sort.Count > 0)
            {
                return sort[0].food;
            }
            return null;
        }

        public static ItemDrop.ItemData GetFoodFromInventoryExcept(List<ItemDrop.ItemData> inventoryItems, List<ItemDrop.ItemData> activeFoodItems)
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

        static List<ScoredFood> GetAllFoods()
        {
            all = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");
            sorted = new List<ScoredFood>();
            foreach (ItemDrop item in all)
            {
                if (item.m_itemData.m_shared.m_food > 0)
                {
                    item.m_itemData.m_dropPrefab = GenerateItemPrefab(item);
                    Log($"Adding: {item.m_itemData.m_dropPrefab.name}", LogLevel.Debug);
                    sorted.Add(GetScoredFood(item.m_itemData));
                }
            }
            sorted.Sort();
            return sorted;
        }

        // The item prefab is added by various methods when items are created in the game.
        // The item's prefab name is used when consuming food.
        // An error will be thrown when prefab is null.
        static GameObject GenerateItemPrefab(ItemDrop item)
        {
            string prefabName = item.GetPrefabName(item.gameObject.name);
            return ObjectDB.instance.GetItemPrefab(prefabName);
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

        static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }
    }
}