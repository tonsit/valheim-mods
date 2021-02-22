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

        private static ManualLogSource logger;

        const string GUID = "org.tonsit.Glutton";

        const string NAME = "Glutton";

        const string VERSION = "0.0.2";

        void Awake()
        {

            percentageRemainingConfig = Config.Bind("Glutton", "FoodDurationRemainingPercent", (uint) 50,
                "The percentage of the food duration at which to attempt to consume the same food. Range: 1-50");
            removeInventoryConfig = Config.Bind("Glutton", "RemoveItemConsumedFromInventory", true,
                "Whether the food consumed by Glutton should reduce the stack count in your inventory.");
            logger = Logger;
            Log("Glutton loaded.");

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
            return removeInventoryConfig.Value;
        }
    }
}