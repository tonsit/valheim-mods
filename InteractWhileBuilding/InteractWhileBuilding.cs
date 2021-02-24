using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace InteractWhileBuilding
{
    [HarmonyPatch]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class InteractWhileBuilding : BaseUnityPlugin
    {
        const string GUID = "org.tonsit.interactwhilebuilding";

        const string NAME = "InteractWhileBuilding";

        const string VERSION = "0.0.1";

        private static ManualLogSource logger;

        void Awake()
        {

            logger = Logger;
            Log("InteractWhileBuilding loaded.");

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
                if (il[i].Calls(InPlaceMode))
                {
                    il[i - 1].opcode = OpCodes.Nop;
                    il[i] = new CodeInstruction(OpCodes.Ldc_I4_0);
                    Log("Modified InPlaceMode");
                }
            }

            return il.AsEnumerable();            
        }

        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            logger.Log(level, data);
        }
    }
}
