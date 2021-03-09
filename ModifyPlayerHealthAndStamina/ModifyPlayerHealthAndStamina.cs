using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace ModifyPlayerHealthAndStamina
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class ModifyPlayerHealthAndStamina : BaseUnityPlugin
    {

        static ConfigEntry<float> playerHealth;
        static ConfigEntry<float> playerStamina;

        static ManualLogSource logger;

        const string GUID = "org.tonsit.modifyplayerhealthandstamina";
        const string NAME = "ModifyPlayerHealthAndStamina";
        const string VERSION = "0.0.1"; 
        
        void Awake()
        {
            string sectionName = "Base stats";

            playerHealth= Config.Bind(sectionName, "Player Health", 25f,
                new ConfigDescription($"Modify player base health points.",
                    null,
                    new ConfigurationManagerAttributes { Order = 9 }));

            playerStamina= Config.Bind(sectionName, "Player Stamina", 75f,
                new ConfigDescription($"Modify player base stamina points.",
                    null,
                    new ConfigurationManagerAttributes { Order = 9 }));

            logger = Logger;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        public static float GetConfigPlayerHealth()
        {
            return playerHealth.Value; 
        }

        public static float GetConfigPlayerStamina()
        {
            return playerStamina.Value;
        }

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            logger.Log(level, data);
        }
    }

    public static class LogExtension
    {
        public static CodeMatcher Log(this CodeMatcher matcher, object data, LogLevel level = LogLevel.Info)
        {
            ModifyPlayerHealthAndStamina.Log(data, level);
            return matcher;
        }
    }
}
