using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Glutton
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Glutton : BaseUnityPlugin
    {
        static ConfigEntry<bool> automaticEatingSwitch;
        static ConfigEntry<uint> percentageRemainingConfig;
        static ConfigEntry<bool> removeInventoryConfig;
        static ConfigEntry<bool> ignoreInventoryConfig;
        static ConfigEntry<bool> eatMaximumFoods;
        static ConfigEntry<bool> eatBestFoodsFirst;
        static ConfigEntry<uint> maximumFoodCount;
        static ConfigEntry<int> foodHealthScoreWeight;
        static ConfigEntry<int> foodBurnTimeScoreWeight;
        static ConfigEntry<int> foodStaminaScoreWeight;
        static ConfigEntry<int> foodRegenScoreWeight;
        static ConfigEntry<bool> normalizeFoodBenefits;
        static ConfigEntry<uint> foodHealthPercentage;
        static ConfigEntry<uint> foodStaminaPercentage;
        static ConfigEntry<DurationTypes> foodDuration;

        static ConfigEntry<KeyboardShortcut> automaticEating { get; set; }
        static ManualLogSource logger;

        const string GUID = "org.tonsit.glutton";
        const string NAME = "Glutton";
        const string VERSION = "1.0.1";
        enum DurationTypes
        {
            Infinite,
            Longer,
            Long,
            Normal,
            Short,
            Shorter,
        }

        void Awake()
        {
            string sectionName = "General";

            automaticEatingSwitch = Config.Bind(sectionName, "Auto Eat", true,
                new ConfigDescription($"Allow {NAME} to eat food.",
                    null,
                    new ConfigurationManagerAttributes { Order = 99 }));

            percentageRemainingConfig = Config.Bind(sectionName, "Refresh Food at Percentage", (uint) 50,
                new ConfigDescription("The percentage of the food duration at which to attempt to eat the same food. Range: 1-50",
                    new AcceptableValueRange<uint>(1, 50),
                    new ConfigurationManagerAttributes { Order = 98 }));

            eatMaximumFoods = Config.Bind(sectionName, "Eat Maximum Foods", true,
                new ConfigDescription($"Whether {NAME} should try to keep maximum food buffs applied.",
                    null,
                    new ConfigurationManagerAttributes { Order = 97 }));

            eatBestFoodsFirst = Config.Bind(sectionName, "Eat Best Foods First", true,
                new ConfigDescription($"When 'Eat Maximum Foods' is enabled, eat foods with best scores first. Set this to false to eat the lowest scored foods first.",
                    null,
                    new ConfigurationManagerAttributes { Order = 96 }));

            ignoreInventoryConfig = Config.Bind(sectionName, "Ignore Inventory", false,
                new ConfigDescription($"Serve any food available in the game, regardless of your inventory",
                    null,
                    new ConfigurationManagerAttributes { Order = 95 }));

            maximumFoodCount = Config.Bind(sectionName, "**Maximum Food Count", 3u,
                new ConfigDescription("How many foods can be active at one time. Note: The UI does not support more than 3 buffs, however the increased stamina and health will still be applied. Restart required.",
                    new AcceptableValueRange<uint>(0, 25),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 31 }));
            
            normalizeFoodBenefits = Config.Bind(sectionName, "**Normalize Food Benefits", false,
                new ConfigDescription($"When enabled food will provide a static buff rather than provide less benefit over time. Defaults to 50% of maximum food value (Average benefit over its duration). HP and Stamina benefits may be adjusted in advanced options. Restart Required",
                    null,
                    new ConfigurationManagerAttributes { Order = 45 }));

            foodHealthPercentage = Config.Bind(sectionName, "**Food Health Benefit", 50u,
                new ConfigDescription($"Set the percentage for the food health. Defaults to 50% of the foods maximum benefit, which is the average of its duration. Setting this value to 100 makes food provide its maximum benefit. Setting this to 0 would make food provide no health benefit. Ignored when Normalize Food is disabled. Restart required.",
                    new AcceptableValueRange<uint>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 42 }));

            foodStaminaPercentage = Config.Bind(sectionName, "**Food Stamina Benefit", 50u,
                new ConfigDescription($"Set the percentage for the food stamina. Defaults to 50% of the foods maximum benefit, which is the average of its duration. Setting this value to 100 makes food provide its maximum stamina benefit. Setting this to 0 would make food provide no stamina benefit. Ignored when Normalize Food is disabled. Restart required.",
                    new AcceptableValueRange<uint>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 41 }));

            foodDuration = Config.Bind(sectionName, "**Food Duration", DurationTypes.Normal,
                new ConfigDescription($"Set the duration for the food timer. This also impacts the rate at which the food benefits decay unless Normalize Food Benefits is enabled. Restart required.",
                null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 40 }));

            removeInventoryConfig = Config.Bind(sectionName, "Remove Eaten Item From Inventory", true,
                new ConfigDescription($"Should eating an item reduce the stack count in your inventory? IgnoreInventory overrides this to false.",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));

            sectionName = "Weighted Food Score";

            foodHealthScoreWeight = Config.Bind(sectionName, "Health Score Weight", 50,
                new ConfigDescription("The multiplier to use when calculating the Food Score.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 4 }));

            foodStaminaScoreWeight = Config.Bind(sectionName, "Stamina Score Weight", 50,
                new ConfigDescription("The multiplier to use when calculating the Food Score.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 3 }));

            foodBurnTimeScoreWeight = Config.Bind(sectionName, "Burn Time Score Weight", 50,
                new ConfigDescription("The multiplier to use when calculating the Food Score.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 2 }));

            foodRegenScoreWeight = Config.Bind(sectionName, "Regen Score Weight", 50,
                new ConfigDescription("The multiplier to use when calculating the Food Score.",
                    new AcceptableValueRange<int>(0, 100),
                    new ConfigurationManagerAttributes { IsAdvanced = true, Order = 1 }));

            sectionName = "Keyboard shortcuts";

            automaticEating = Config.Bind(sectionName, "Toggle Automatic Eating", new KeyboardShortcut(KeyCode.T, KeyCode.LeftShift),
                new ConfigDescription($"Keyboard shortcut to enable/disable {NAME}",
                    null,
                    new ConfigurationManagerAttributes { Order = 60 }));

            logger = Logger;

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Update")]
        static void CheckForKeyStrokes(Player __instance) 
        {
            if (ShouldIgnoreKeyboardInput(__instance))
            {
                return;
            }

            if (automaticEating.Value.IsDown())
            {
                ToggleAutomaticEatingSwitch(__instance);
            }
        }

        static bool ShouldIgnoreKeyboardInput(Player __instance)
        {
            return ((Player.m_localPlayer != __instance)
                || Console.IsVisible()
                || TextInput.IsVisible()
                || Minimap.InTextInput()
                || Menu.IsVisible()
                || (Chat.instance != null && Chat.instance.IsChatDialogWindowVisible()));
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

        public static bool GetConfigEatMaximumFoods()
        {
            return eatMaximumFoods.Value;
        }

        public static uint GetConfigMaximumFoodCount()
        {
            return maximumFoodCount.Value;
        }

        public static bool GetConfigEatBestFoodsFirst()
        {
            return eatBestFoodsFirst.Value;
        }

        public static float GetConfigFoodHealthScoreWeight()
        {
            return foodHealthScoreWeight.Value;
        }
        public static float GetConfigFoodBurnTimeScoreWeight()
        {
            return foodBurnTimeScoreWeight.Value / 50;
        }
        public static float GetConfigFoodRegenScoreWeight()
        {
            return foodRegenScoreWeight.Value * 10;
        }
        public static float GetConfigFoodStaminaScoreWeight()
        {
            return foodStaminaScoreWeight.Value;
        }

        public static bool GetAutomaticEatingSwitch()
        {
            return automaticEatingSwitch.Value;
        }

        static void ToggleAutomaticEatingSwitch(Player player)
        {
            if (automaticEatingSwitch.Value)
            {
                automaticEatingSwitch.Value = false;
            }
            else
            {
                automaticEatingSwitch.Value = true;
            }
            Flash(player, $"Auto Eat: {automaticEatingSwitch.Value}");
        }

        public static void Flash(Player player, string message)
        {
            Log(message);
            player.Message(MessageHud.MessageType.Center, message);
        }

        public static float GetConfigFoodDurationMultiplier()
        {
            switch (foodDuration.Value)
            {
                case DurationTypes.Infinite:
                    return 0f;
                case DurationTypes.Longer:
                    return .1f;
                case DurationTypes.Long:
                    return .5f;
                case DurationTypes.Short:
                    return 2f;
                case DurationTypes.Shorter:
                    return 20f;
                case DurationTypes.Normal:
                default:
                    return 1f;
            }
        }

            public static bool GetConfigNormalizeFoodBenefit()
        {
            return normalizeFoodBenefits.Value;
        }

        public static float GetConfigFoodHealthMultiplier()
        {
            return (float) foodHealthPercentage.Value / 100;
        }

        public static float GetConfigFoodStaminaMultiplier()
        {
            return (float) foodStaminaPercentage.Value / 100;
        }
    }

    public static class LogExtension
    {
        public static CodeMatcher Log(this CodeMatcher matcher, object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
            return matcher;
        }
    }
}