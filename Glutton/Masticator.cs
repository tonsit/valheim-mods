using HarmonyLib;

namespace Glutton
{
    [HarmonyPatch(typeof(Player), "UpdateFood")]
    class Masticator
    {
        static void Prefix(Player __instance)
        {
            foreach (Player.Food food in __instance.GetFoods())
            {
                if (FoodPercentageBelowThreshhold(food))
                {
                    TryToEatMore(__instance, food);
                }
            }
        }

        static ItemDrop.ItemData GetInventoryItem(Player player, Player.Food food)
        {
            return player.GetInventory().GetItem(food.m_item.m_shared.m_name);
        }

    private static bool FoodPercentageBelowThreshhold(Player.Food food)
        {
            
            return GetFoodPercentage(food) < GetConfigPercentage();
        }

        private static float GetFoodPercentage(Player.Food food)
        {
            return 100 * food.m_health / food.m_item.m_shared.m_food;
        }

        private static bool TryToEatMore(Player player, Player.Food food)
        {
            if (HasFoodInInventory(player, food))
            {
                Log($"Consuming {food.m_name}");
                return Chew(player, food);
            }
            return false;
        }

        // Glutton.Masticator
        private static bool HasFoodInInventory(Player player, Player.Food food)
        {
            return player.GetInventory().HaveItem(food.m_item.m_shared.m_name);
        }

        // Glutton.Masticator
        private static bool Chew(Player player, Player.Food food)
        {
            if (GetConfigRemoveInventory()) {
                player.UseItem(player.GetInventory(), GetInventoryItem(player, food), true);
                return true;
            }
            return player.ConsumeItem(player.GetInventory(), food.m_item);
        }

        private static void Log(object data)
        {
            Glutton.Log(data);
        }

        private static uint GetConfigPercentage()
        {
            return Glutton.GetConfigPercentage();
        }

        private static bool GetConfigRemoveInventory()
        {
            return Glutton.GetConfigRemoveInventory();
        }

    }
}
