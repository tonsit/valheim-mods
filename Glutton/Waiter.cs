using HarmonyLib;
using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace Glutton
{
    [HarmonyPatch(typeof(Player), "UpdateFood")]
    class Waiter
    {
        public static float m_foodRefreshTimer { get; private set; }
        public static Player.Food[] foods { get; private set; }

        static void Prefix(Player __instance, float dt)
        {
            m_foodRefreshTimer += dt;
            if (!(m_foodRefreshTimer > 1f))
            {
                return;
            }
            m_foodRefreshTimer = 0f;
            CheckFoodOnPlayer(__instance);
        }

        static void CheckFoodOnPlayer(Player player)
        {
            MaybeEatMoreFoods(player);
            RefreshActiveFood(player);
        }

        static void MaybeEatMoreFoods(Player player)
        {
            if (GetConfigConsumeMaximumFoods()
                && player.GetFoods().Count < GetConfigMaximumFoodCount())
            {
                try
                {
                    List<Player.Food> foods = player.GetFoods();
                    List<ItemDrop.ItemData> foodItems = ConvertFoodToItems(foods);
                    ItemDrop.ItemData bestFood = GetBestFood(player.GetInventory(), foodItems);
                    Log($"Trying to serve more: {bestFood.m_shared.m_name}");
                    TryToServeMore(player, bestFood);
                }
                catch (NullReferenceException)
                {
                    // Player has no best food.
                    Log($"NullReferenceException caught", LogLevel.Debug);
                }
            }
        }

        static void RefreshActiveFood(Player player)
        {
            player.GetFoods().ForEach(food => {
                if (FoodPercentageBelowThreshhold(food))
                {

                    Log($"Found {food.m_name} below threshhold", LogLevel.Debug);
                    List<Player.Food> foods = new List<Player.Food>();
                    player.GetFoods().ForEach(activeFood =>
                    {
                        if (activeFood.m_name != food.m_name)
                        {
                            foods.Add(activeFood);
                        }
                    }
                        );
                    List<ItemDrop.ItemData> foodItems = ConvertFoodToItems(foods);
                    ItemDrop.ItemData item = GetBestFood(player.GetInventory(), foodItems);
                    TryToServeMore(player, item);
                }
            });
        }

        static ItemDrop.ItemData GetBestFood(Inventory inventory, List<ItemDrop.ItemData> foodItems)
        {
            if (GetConfigIgnoreInventory())
            {
                return GetBestFoodInGameExcept(foodItems);
            }
            return GetBestFoodFromInventoryExcept(inventory, foodItems);
        }

        static ItemDrop.ItemData GetBestFoodFromInventoryExcept(Inventory inventory, List<ItemDrop.ItemData> foodItems)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();
            Log($"Get best food from inventory except: {items.Count}", LogLevel.Debug);
            inventory.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, items);
            Log($"Items in inventory: {items.Count}", LogLevel.Debug);
            foreach (ItemDrop.ItemData item in items)
            {
                Log(item.m_shared.m_name, LogLevel.Debug);
            }
            return Kitchen.GetBestFoodFromInventoryExcept(items, foodItems);
        }

        static ItemDrop.ItemData GetBestFoodInGameExcept(List<ItemDrop.ItemData> foodItems)
        {
            Log($"Get best food in game except: {foodItems.Count}", LogLevel.Debug);
            foodItems.ForEach(item => {
                Log($"{item.m_shared.m_name}");
            });
            return Kitchen.GetBestFoodInGameExcept(foodItems);
        }

        static List<ItemDrop.ItemData> ConvertFoodToItems(List<Player.Food> foods)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();
            foreach (Player.Food food in foods)
            {
                items.Add(food.m_item);
            }
            return items;
        }

        private static bool FoodPercentageBelowThreshhold(Player.Food food)
        {
            
            return GetFoodPercentage(food) < GetConfigPercentage();
        }

        private static float GetFoodPercentage(Player.Food food)
        {
            return 100 * food.m_health / food.m_item.m_shared.m_food;
        }

        private static bool TryToServeMore(Player player, ItemDrop.ItemData food)
        {
            Log($"Serving {food.m_dropPrefab.name}", LogLevel.Debug);
            return Masticator.TryToEatMore(player, food);
        }

      
        private static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }

        private static uint GetConfigPercentage()
        {
            return Glutton.GetConfigPercentage();
        }

        private static bool GetConfigIgnoreInventory()
        {
            return Glutton.GetConfigIgnoreInventory();
        }

        private static int GetConfigMaximumFoodCount()
        {
            return Glutton.GetConfigMaximumFoodCount();
        }

        private static bool GetConfigConsumeMaximumFoods()
        {
            return Glutton.GetConfigConsumeMaximumFoods();
        }
    }
}
