using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;


namespace Glutton
{
    [HarmonyPatch]
    class ModifyFoodsCount
    {
        private static MethodInfo GetCount = AccessTools.Method(typeof(List<Player.Food>), "get_Count");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "EatFood")]
        static IEnumerable<CodeInstruction> PatchEatFood(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();
            for (int i = 2; i < il.Count; ++i)
            {
                //  IL_00a7: callvirt instance int32 class [mscorlib] System.Collections.Generic.List`1<class Player/Food>::get_Count()
                //  IL_00ac: ldc.i4.3
                //  IL_00ad: bge.s IL_010f
                if (il[i - 2].Calls(GetCount)
                    && il[i - 1].opcode == OpCodes.Ldc_I4_3
                    && il[i].opcode == OpCodes.Bge)
                {
                    il[i - 1] = new CodeInstruction(OpCodes.Ldc_I4, GetConfigMaximumFoodCount());
                    Log("Modified EatFood");
                }
            }

            return il.AsEnumerable();            
        }
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "CanEat")]
        static IEnumerable<CodeInstruction> PatchCanEat(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();
            for (int i = 2; i < il.Count; ++i)
            {
                // IL_00cf: callvirt instance int32 class [mscorlib] System.Collections.Generic.List`1<class Player/Food>::get_Count()
                // IL_00d4: ldc.i4.3
                // IL_00d5: blt.s IL_00e7
                if (il[i - 2].Calls(GetCount)
                    && (il[i - 1].opcode == OpCodes.Ldc_I4_3)
                    && il[i].opcode == OpCodes.Blt)
                {
                    il[i - 1] = new CodeInstruction(OpCodes.Ldc_I4, GetConfigMaximumFoodCount());
                    Log("Modified CanEat");
                }
            }

            return il.AsEnumerable();
        }

        public static int GetConfigMaximumFoodCount()
        {
            return Glutton.GetConfigMaximumFoodCount();
        }


        public static void Log(object data, LogLevel level = LogLevel.Info)
        {
            Glutton.Log(data, level);
        }
    }
}
