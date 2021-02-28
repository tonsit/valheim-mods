using HarmonyLib;
using BepInEx.Logging;

namespace Glutton
{
    [HarmonyPatch(typeof(Player), "UpdateFood")]
    class Masticator
    {

        public static bool TryToEatMore(Player player, ItemDrop.ItemData food)
        {
            if (HasFoodInInventory(player.GetInventory(), food))
            {
                Log($"Chewing: {food.m_dropPrefab.name}", LogLevel.Debug);
                return Chew(player, food);
            }
            Log("No food in inventory", LogLevel.Debug);
            return false;
        }

        static ItemDrop.ItemData GetInventoryItem(Player player, ItemDrop.ItemData food)
        {
            return player.GetInventory().GetItem(food.m_shared.m_name);
        }

        private static bool HasFoodInInventory(Inventory inventory, ItemDrop.ItemData food)
        {
            return GetConfigIgnoreInventory() || inventory.HaveItem(food.m_shared.m_name);
        }

          private static bool Chew(Player player, ItemDrop.ItemData food)
        {
            if (GetConfigRemoveInventory()) {
                return ConsumeFromInventory(player, food);
            }
            return ConsumeFromKitchen(player, food);
        }

        private static bool ConsumeFromInventory(Player player, ItemDrop.ItemData food)
        {
            //Localization.instance.Localize(food.m_shared.m_name);
            Log($"Consuming from inventory {food.m_shared.m_name}");
            player.UseItem(player.GetInventory(), GetInventoryItem(player, food), true);
            return true;
        }

        static bool ConsumeFromKitchen(Player player, ItemDrop.ItemData food)
        {
            Log($"Consuming from kitchen {food.m_dropPrefab.name}", LogLevel.Debug);
            Log($"Consumable: {player.CanConsumeItem(food)}", LogLevel.Debug);
            Log($"ConsumeStatusEffect: {food.m_shared.m_consumeStatusEffect}", LogLevel.Debug);
            Log($"m_shared.m_food: {food.m_shared.m_food}", LogLevel.Debug);
            player.EatFood(food);
            Log($"Food Count: {player.GetFoods().Count}");
            return true;
        }

        private static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }

        private static bool GetConfigRemoveInventory()
        {
            return Glutton.GetConfigRemoveInventory();
        }

        private static bool GetConfigIgnoreInventory()
        {
            return Glutton.GetConfigIgnoreInventory();
        }
    }
}
