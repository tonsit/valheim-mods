using BepInEx.Logging;

namespace Glutton
{
    class Masticator
    {

        public static bool TryToEatMore(Player player, ItemDrop.ItemData food)
        {
            if (HasFoodInInventory(player.GetInventory(), food))
            {
                Log($"Chewing: {food.m_dropPrefab.name}", LogLevel.Debug, 2);
                return Chew(player, food);
            }
            Log("No food in inventory", LogLevel.Debug);
            return false;
        }

        static ItemDrop.ItemData GetInventoryItem(Player player, ItemDrop.ItemData food)
        {
            return player.GetInventory().GetItem(food.m_shared.m_name);
        }

        static bool HasFoodInInventory(Inventory inventory, ItemDrop.ItemData food)
        {
            return GetConfigIgnoreInventory() || inventory.HaveItem(food.m_shared.m_name);
        }

        static bool Chew(Player player, ItemDrop.ItemData food)
        {
            if (GetConfigRemoveInventory()) {
                return ConsumeFromInventory(player, food);
            }
            return ConsumeFromKitchen(player, food);
        }

        static bool ConsumeFromInventory(Player player, ItemDrop.ItemData food)
        {
            //Localization.instance.Localize(food.m_shared.m_name);
            Log($"Consuming from inventory {food.m_dropPrefab.name}");
            player.UseItem(player.GetInventory(), GetInventoryItem(player, food), true);
            return true;
        }

        static bool ConsumeFromKitchen(Player player, ItemDrop.ItemData food)
        {
            Log($"Consuming from kitchen {food.m_dropPrefab.name}", LogLevel.Debug);
            player.EatFood(food);
            Log($"Food Count: {player.GetFoods().Count}", LogLevel.Debug, 2);
            return true;
        }

        static void Log(object data, LogLevel level = LogLevel.Info, uint verbosity = 1)
        {
            Glutton.Log(data, level, verbosity);
        }

        static bool GetConfigRemoveInventory()
        {
            return Glutton.GetConfigRemoveInventory();
        }

        static bool GetConfigIgnoreInventory()
        {
            return Glutton.GetConfigIgnoreInventory();
        }
    }
}
