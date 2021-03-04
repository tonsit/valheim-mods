using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using BepInEx.Configuration;

namespace InteractWhileBuilding
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class InteractWhileBuilding : BaseUnityPlugin
    {
        const string GUID = "org.tonsit.interactwhilebuilding";

        const string NAME = "InteractWhileBuilding";

        const string VERSION = "1.0.0";

        private static ManualLogSource logger;

        static ConfigEntry<float> configXPadding;
        static ConfigEntry<float> configYPadding;

        void Awake()
        {
            logger = Logger;
            Log("InteractWhileBuilding loaded.");

            string sectionName = "Padding for WearNTear GUI";

            configXPadding = Config.Bind(sectionName, "X-axis", 0f,
                new ConfigDescription($"Modify the horizontal positioning of the element",
                    new AcceptableValueRange<float>(-1000, 1000)));

            configYPadding = Config.Bind(sectionName, "Y-axis", 20f,
                new ConfigDescription($"Modify the vertical positioning of the element",
                    new AcceptableValueRange<float>(-1000, 1000)));

            var harmony = new Harmony(GUID);
            harmony.PatchAll();
        }

        private static MethodInfo InPlaceMode = AccessTools.Method(typeof(Character), "InPlaceMode");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "UpdateHover")]
        static IEnumerable<CodeInstruction> Patch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();
            for (int i = 0; i < il.Count; ++i)
            {
                // IL_0000: ldarg.0
                // IL_0001: callvirt instance bool Character::InPlaceMode()
                // IL_0006: brtrue.s IL_001e
                if (il[i].Calls(InPlaceMode))
                {
                    il[i - 1].opcode = OpCodes.Nop;
                    il[i] = new CodeInstruction(OpCodes.Ldc_I4_0);
                    Log("Modified InPlaceMode");
                }
            }

            return il.AsEnumerable();            
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), "Awake")]
        private static void BumpUpPieceHealthRoot(Hud __instance)
        {
            Vector3 position = __instance.m_pieceHealthRoot.localPosition;
            __instance.m_pieceHealthRoot.localPosition = new Vector3(position.x + GetConfigXPadding(), position.y + GetConfigYPadding(), position.z);
        }

        static float GetConfigXPadding()
        {
            return configXPadding.Value;
        }

        static float GetConfigYPadding()
        {
            return configYPadding.Value;
        }

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            logger.Log(level, data);
        }
    }
}
