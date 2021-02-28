using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace Glutton
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Glutton : BaseUnityPlugin
    {
        private static ConfigEntry<uint> percentageRemainingConfig;
        private static ConfigEntry<bool> removeInventoryConfig;
        private static ConfigEntry<bool> ignoreInventoryConfig;
        private static ConfigEntry<bool> consumeMaximumFoods;
        private static ConfigEntry<bool> allowDoubleDip;
        private static ConfigEntry<int> maximumFoodCount;

        private static ManualLogSource logger;

        const string GUID = "org.tonsit.Glutton";

        const string NAME = "Glutton";

        const string VERSION = "0.1.0";

        void Awake()
        {
            percentageRemainingConfig = Config.Bind(NAME, "FoodDurationRemainingPercent", (uint) 50,
                "The percentage of the food duration at which to attempt to consume the same food. Range: 1-50");
            consumeMaximumFoods = Config.Bind(NAME, "ConsumeMaximumFoods", true,
                "Whether Glutton should try to keep maximum food buffs applied.");
            maximumFoodCount = Config.Bind(NAME, "MaximumFoodCount", 5,
                "How many foods can be active at one time?");
            ignoreInventoryConfig = Config.Bind(NAME, "IgnoreInventory", false,
                "Whether Glutton should consume the best food available in the game regardless of your inventory.");
            removeInventoryConfig = Config.Bind(NAME, "RemoveItemConsumedFromInventory", true,
                "Whether Glutton should reduce the stack count in your inventory when consuming food. IgnoreInventory overrides this setting to false.");
            allowDoubleDip = Config.Bind(NAME, "AllowDoubleDip", true,
                "Allow more than one of a single food buff");

            logger = Logger;

            var harmony = new Harmony("mod.glutton");
            harmony.PatchAll();
        }

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            logger.Log(level, data);
        }

        public static uint GetConfigPercentage()
        {
            return percentageRemainingConfig.Value;
        }

        public static bool GetConfigRemoveInventory()
        {
            if (GetConfigIgnoreInventory() || ! removeInventoryConfig.Value)
            {
                return false;
            }
            return true;
        }

        public static bool GetConfigIgnoreInventory()
        {
            return ignoreInventoryConfig.Value;
        }

        public static bool GetConfigConsumeMaximumFoods()
        {
            return consumeMaximumFoods.Value;
        }

        public static int GetConfigMaximumFoodCount()
        {
            return maximumFoodCount.Value;
        }

        public static bool GetConfigAllowDoubleDip()
        {
            return allowDoubleDip.Value;
        }
    }
}