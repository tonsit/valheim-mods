using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace SafetyNet
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class SafetyNet : BaseUnityPlugin
    {
        const string GUID = "org.tonsit.safetynet";

        const string NAME = "SafetyNet";

        const string VERSION = "0.0.1";

        private static ConfigEntry<bool> takesFallDamage;

        private static ConfigEntry<bool> takesFireDamage;

        private static ConfigEntry<bool> takesFreezeDamage;

        private static ConfigEntry<bool> takesSmokeDamage;

        private static ConfigEntry<bool> takesSwimDamage;

        private static ManualLogSource logger;

        void Awake()
        {
            string message = "Whether to take the damage";

            takesFallDamage = Config.Bind(NAME, "TakesFallDamage", false, message);
            takesFireDamage = Config.Bind(NAME, "TakesFireDamage", true, message);
            takesFreezeDamage = Config.Bind(NAME, "TakesFreezeDamage", true, message);
            takesSmokeDamage = Config.Bind(NAME, "TakesSmokeDamage", true, message);
            takesSwimDamage = Config.Bind(NAME, "TakesSwimDamage", false, message);

            logger = Logger;
            Log("SafetyNet loaded.");

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        static void ThrowSafetyNet(Character __instance, ref HitData hit, bool showDamageText, bool triggerEffects, HitData.DamageModifier mod = HitData.DamageModifier.Normal)
        {
            EnableTolerateSettings(__instance);
            CheckForEnvironmentDamage(__instance, ref hit);
        }

        static void EnableTolerateSettings(Character character)
        {
            if (!takesSmokeDamage.Value) {
                character.m_tolerateSmoke = true;
            }
            if (!takesFireDamage.Value)
            {
                character.m_tolerateFire = true;
            }
            if (!takesSwimDamage.Value)
            {
                character.m_tolerateWater = true;
            }
        }

        static void CheckForEnvironmentDamage(Character character, ref HitData hit)
        {
            if (DamageCameFromLanding(hit) && !takesFallDamage.Value)
            {
                Log($"Fall: {hit.m_damage.m_damage}");
                SetDamageToZero(ref hit);
            }
            if (DamageCameFromBurning(character, hit) && !takesFireDamage.Value)
            {
                Log($"Burning: {hit.m_damage.m_fire}");
                SetFireDamageToZero(ref hit);
            }
            if (DamageCameFromWater(character, hit) && !takesSwimDamage.Value)
            {
                Log($"Water: {hit.m_damage.m_damage}");
                SetDamageToZero(ref hit);
            }
            if (DamageCameFromSmoked(character, hit) && !takesSmokeDamage.Value)
            {
                Log($"Smoked: {hit.m_damage.m_damage}");
                SetDamageToZero(ref hit);
            }
            if (DamageCameFromFreezing(character, hit) && !takesFreezeDamage.Value)
            {
                Log($"Freezing: {hit.m_damage.m_damage}");
                SetDamageToZero(ref hit);
            }

        }

        static bool DamageCameFromLanding(HitData hit)
        {
            return (hit.m_damage.m_damage > 0
                && hit.m_dir.y > 0
                && hit.m_attacker == ZDOID.None);
        }

        static bool DamageCameFromBurning(Character character, HitData hit)
        {
            return (hit.m_damage.m_fire > 0
                && hit.m_attacker == ZDOID.None
                && HaveStatusEffect(character, "Burning"));
        }

        static bool DamageCameFromWater(Character character, HitData hit)
        {
            return (hit.m_damage.m_damage > 0
                && hit.m_dir.y < 0
                && hit.m_attacker == ZDOID.None
                && HaveStatusEffect(character, "Wet")
                && character.IsSwiming());
        }

        static bool DamageCameFromSmoked(Character character, HitData hit)
        {
            return (hit.m_damage.m_damage > 0
                && hit.m_dir.y == 0
                && hit.m_attacker == ZDOID.None
                && HaveStatusEffect(character, "Smoked"));
        }

        static bool DamageCameFromFreezing(Character character, HitData hit)
        {
            return (hit.m_damage.m_damage > 0
                && hit.m_dir.y == 0
                && hit.m_attacker == ZDOID.None
                && HaveStatusEffect(character, "Freezing"));
        }

        static bool HaveStatusEffect(Character character, string statusEffectName)
        {
            return character.GetSEMan().HaveStatusEffect(statusEffectName);
        }

        static void SetDamageToZero(ref HitData hit)
        {
            hit.m_damage.m_damage = 0;
        }

        static void SetFireDamageToZero(ref HitData hit)
        {
            hit.m_damage.m_fire = 0;
        }

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            logger.Log(level, data);
        }
    }
}
