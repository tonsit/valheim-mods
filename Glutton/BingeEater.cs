using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Glutton
{
    [HarmonyPatch]
    class BingeEater
    {
        const int MAXIMUM_FOOD_COUNT = 3;

        static MethodInfo GetCount = AccessTools.Method(typeof(List<Player.Food>), "get_Count");

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "EatFood")]
        static IEnumerable<CodeInstruction> PatchEatFoodMore(IEnumerable<CodeInstruction> instructions)
        {
            //   IL_00a7: callvirt instance int32 class [mscorlib] System.Collections.Generic.List`1<class Player/Food>::get_Count()
            //-- IL_00ac: ldc.i4.3
            //++ ldc.i4 MaximumFoodCount
            //   IL_00ad: bge.s IL_010f
            return MaximumFoodCountIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.Calls(GetCount)),
                    new CodeMatch(OpCodes.Ldc_I4_3),
                    new CodeMatch(OpCodes.Bge))
                .Advance(1)
                .Set(OpCodes.Ldc_I4, (int)GetConfigMaximumFoodCount())
                .Log($"Modified Player.EatFood -- Binge Eating: {GetConfigMaximumFoodCount()}")
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "CanEat")]
        static IEnumerable<CodeInstruction> PatchCanEatMore(IEnumerable<CodeInstruction> instructions)
        {
            //   IL_00cf: callvirt instance int32 class [mscorlib] System.Collections.Generic.List`1<class Player/Food>::get_Count()
            //-- IL_00d4: ldc.i4.3
            //++ ldc.i4 MaximumFoodCount
            //   IL_00d5: blt.s IL_00e7
            return MaximumFoodCountIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.Calls(GetCount)),
                    new CodeMatch(OpCodes.Ldc_I4_3),
                    new CodeMatch(OpCodes.Blt))
                .Advance(1)
                .Set(OpCodes.Ldc_I4, (int)GetConfigMaximumFoodCount())
                .Log($"Modified Player.CanEat -- Binge Eating: {GetConfigMaximumFoodCount()}")
                .InstructionEnumeration();
        }

        static bool MaximumFoodCountIsUnaltered()
        {
            return GetConfigMaximumFoodCount() == MAXIMUM_FOOD_COUNT;
        }

        static uint GetConfigMaximumFoodCount()
        {
            return Glutton.GetConfigMaximumFoodCount();
        }
    }
}
