using HarmonyLib;
using System;
using System.Collections.Generic;
using BepInEx.Logging;
using System.Reflection;
using System.Reflection.Emit;

namespace Glutton
{
    [HarmonyPatch]
    class Waiter
    {
        public static float m_foodRefreshTimer { get; private set; }
        public static Player.Food[] foods { get; private set; }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "UpdateFood")]
        static void Begin(Player __instance, float dt)
        {
            m_foodRefreshTimer += dt;
            if (!(m_foodRefreshTimer > 1f) || AutomaticEatingIsDisabled())
            {
                return;
            }
            m_foodRefreshTimer = 0f;
            CheckFoodOnPlayer(__instance);
        }

        static bool AutomaticEatingIsDisabled()
        {
            return !Glutton.GetAutomaticEatingSwitch();
        }

        static void CheckFoodOnPlayer(Player player)
        {
            MaybeEatMoreFoods(player);
            RefreshActiveFood(player);
        }

        static void MaybeEatMoreFoods(Player player)
        {
            if (GetConfigEatMaximumFoods()
                && player.GetFoods().Count < GetConfigMaximumFoodCount())
            {
                try
                {
                    List<Player.Food> foods = player.GetFoods();
                    List<ItemDrop.ItemData> foodItems = ConvertFoodToItems(foods);
                    ItemDrop.ItemData food = GetFood(player.GetInventory(), foodItems);
                    Log($"Trying to serve more: {food.m_shared.m_name}");
                    TryToServeMore(player, food);
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
            foreach (Player.Food food in player.GetFoods())
            {
                Log($"Active food hp//sta: {food.m_name} :: {food.m_health} // {food.m_stamina} :: player hp//sta :: {player.GetHealth()} // {player.GetStamina()}", LogLevel.Debug);
                if (FoodPercentageBelowThreshhold(food))
                {

                    Log($"Found {food.m_name} below threshold", LogLevel.Debug);
                    List<Player.Food> foods = new List<Player.Food>();
                    player.GetFoods().ForEach(activeFood =>
                    {
                        if (activeFood.m_name != food.m_name)
                        {
                            foods.Add(activeFood);
                        }
                    });
                    List<ItemDrop.ItemData> foodItems = ConvertFoodToItems(foods);
                    ItemDrop.ItemData item =
                        GetConfigEatMaximumFoods()
                        ? GetFood(player.GetInventory(), foodItems)
                        : food.m_item;
                    try
                    {
                        TryToServeMore(player, item);
                    }
                    catch (NullReferenceException)
                    {
                        // Player has no best food.
                        Log($"NullReferenceException caught", LogLevel.Debug);
                    }
                }
            }
        }

        static ItemDrop.ItemData GetFood(Inventory inventory, List<ItemDrop.ItemData> foodItems)
        {
            if (GetConfigIgnoreInventory())
            {
                return GetFoodFromKitchenExcept(foodItems);
            }
            return GetFoodFromInventoryExcept(inventory, foodItems);
        }

        static ItemDrop.ItemData GetFoodFromInventoryExcept(Inventory inventory, List<ItemDrop.ItemData> foodItems)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();
            items = inventory.GetAllFoods();
            Log($"Get best food from inventory except: {items.Count}", LogLevel.Debug);
            Log($"Items in inventory: {items.Count}", LogLevel.Debug);
            foreach (ItemDrop.ItemData item in items)
            {
                Log(item.m_shared.m_name, LogLevel.Debug);
            }
            return Kitchen.GetFoodFromInventoryExcept(items, foodItems);
        }

        static ItemDrop.ItemData GetFoodFromKitchenExcept(List<ItemDrop.ItemData> foodItems)
        {
            Log($"Get best food in game except: {foodItems.Count}", LogLevel.Debug);
            foodItems.ForEach(item => {
                Log($"{item.m_shared.m_name}", LogLevel.Debug);
            });
            return Kitchen.GetFoodFromKitchenExcept(foodItems);
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

        static bool FoodPercentageBelowThreshhold(Player.Food food)
        {

            return GetFoodPercentage(food) < GetConfigPercentage();
        }

        static float GetFoodPercentage(Player.Food food)
        {
            return 100 * food.m_health / food.m_item.m_shared.m_food;
        }

        static bool TryToServeMore(Player player, ItemDrop.ItemData food)
        {
            Log($"Serving {food.m_dropPrefab.name}", LogLevel.Debug);
            return Masticator.TryToEatMore(player, food);
        }

        static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }

        static uint GetConfigPercentage()
        {
            return Glutton.GetConfigPercentage();
        }

        static bool GetConfigIgnoreInventory()
        {
            return Glutton.GetConfigIgnoreInventory();
        }

        static uint GetConfigMaximumFoodCount()
        {
            return Glutton.GetConfigMaximumFoodCount();
        }

        static bool GetConfigEatMaximumFoods()
        {
            return Glutton.GetConfigEatMaximumFoods();
        }
    }

    public static class InventoryExtension
    {
        public static List<ItemDrop.ItemData> GetAllFoods(this Inventory inventory)
        {
            List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>();
            List<ItemDrop.ItemData> foods = new List<ItemDrop.ItemData>();
            inventory.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, items);
            foreach (ItemDrop.ItemData item in items)
            {
                if (item.m_shared.m_food > 0)
                {
                    foods.Add(item);
                }
            }
            return foods;
        }

    }
    [HarmonyPatch]
    class Normalizer
    {
        public static float m_foodRefreshTimer { get; private set; }
        public static Player.Food[] foods { get; private set; }

        static FieldInfo m_item = AccessTools.Field(typeof(Player.Food), "m_item");
        static FieldInfo m_health = AccessTools.Field(typeof(Player.Food), "m_health");
        static FieldInfo m_stamina = AccessTools.Field(typeof(Player.Food), "m_stamina");
        static FieldInfo m_shared = AccessTools.Field(typeof(ItemDrop.ItemData), "m_shared");
        static FieldInfo m_food = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), "m_food");
        static FieldInfo m_foodStamina = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), "m_foodStamina");
        static FieldInfo m_foodBurnTime = AccessTools.Field(typeof(ItemDrop.ItemData.SharedData), "m_foodBurnTime");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "UpdateFood")]
        static IEnumerable<CodeInstruction> AdjustFoodDuration(IEnumerable<CodeInstruction> instructions)
        {
            //   IL_006b: ldfld float32 ItemDrop/ItemData/SharedData::m_foodBurnTime
            //   IL_0070: div
            //++ float FoodDuration()
            //++ mul
            //   IL_0071: sub
            //   IL_0072: stfld float32 Player / Food::m_health
            return FoodDurationIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.LoadsField(m_foodBurnTime)),
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Sub),
                    new CodeMatch(i => i.StoresField(m_health) || i.StoresField(m_stamina)))
                .Advance(2)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldc_R4, GetConfigFoodDurationMultiplier()),
                    new CodeInstruction(OpCodes.Mul))
                .Log($"Modified Player.UpdateFood -- Duration: {1 / GetConfigFoodDurationMultiplier() * 100}%")
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        static IEnumerable<CodeInstruction> AdjustFoodBenefits(IEnumerable<CodeInstruction> instructions)
        {
            //   // hp = 25f;
            //   IL_0000: ldarg.1
            //   IL_0001: ldc.r4 25
            //   IL_0006: stind.r4
            //   // stamina = 75f;
            //   _0007: ldarg.2
            //IL_0008: ldc.r4 75
            //IL_000d: stind.r4

            //   // hp += food.m_health;
            //-- IL_0028: ldfld float32 Player/Food::m_health
            //++ ldfld float32 PlayerFood::m_item.ItemData.ItemDrop.m_food
            //++ float food_health_multiplier
            //++ mul
            //   IL_002d: add
            //   IL_002e: stind.r4
            //   //stamina += food.m_stamina;
            //-- IL_0033: ldfld float32 Player/Food::m_stamina
            //++ ldfld float32 PlayerFood::m_item.ItemData.ItemDrop.m_foodStamina
            //++ float food_health_multiplier
            //++ mul
            //   IL_0038: add
            //   IL_0039: stind.r4
            //instruc FoodBenefitIsUnaltered() ? instructions : new CodeMatcher(instructions)
            //    .MatchForward(false,
            //        new CodeMatch(i => i.IsLdarg(1)),
            //        new CodeMatch(OpCodes.Ldc_R4, 25f),
            //        new CodeMatch(OpCodes.Stind_R4)
            //    )
            return FoodBenefitIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.LoadsField(m_health)))
                .SetOperandAndAdvance(m_item)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldfld, m_shared),
                    new CodeInstruction(OpCodes.Ldfld, m_food),
                    new CodeInstruction(OpCodes.Ldc_R4, GetConfigFoodHealthMultiplier()),
                    new CodeInstruction(OpCodes.Mul))
                .MatchForward(false,
                    new CodeMatch(i => i.LoadsField(m_stamina)))
                .SetOperandAndAdvance(m_item)
                .InsertAndAdvance(
                    new CodeInstruction(OpCodes.Ldfld, m_shared),
                    new CodeInstruction(OpCodes.Ldfld, m_foodStamina),
                    new CodeInstruction(OpCodes.Ldc_R4, GetConfigFoodStaminaMultiplier()),
                    new CodeInstruction(OpCodes.Mul))
                .Log($"Modified Player.GetTotalFoodValue -- Normalized: {GetConfigFoodHealthMultiplier() * 100}% hp // {GetConfigFoodStaminaMultiplier() * 100}% stamina")
                .InstructionEnumeration();
        }

        static bool FoodDurationIsUnaltered()
        {
            return GetConfigFoodDurationMultiplier() == 1;
        }
        static float GetConfigFoodDurationMultiplier()
        {
            return Glutton.GetConfigFoodDurationMultiplier();
        }

        static bool FoodBenefitIsUnaltered()
        {
            return !Glutton.GetConfigNormalizeFoodBenefit();
        }

        static float GetConfigFoodHealthMultiplier()
        {
            return Glutton.GetConfigFoodHealthMultiplier();
        }

        static float GetConfigFoodStaminaMultiplier()
        {
            return Glutton.GetConfigFoodStaminaMultiplier();
        }
    }
}
