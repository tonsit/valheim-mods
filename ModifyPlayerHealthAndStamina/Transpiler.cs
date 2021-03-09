using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ModifyPlayerHealthAndStamina
{
    [HarmonyPatch]
    class PlayerBase
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        static IEnumerable<CodeInstruction> PatchGetTotalFoodValue(IEnumerable<CodeInstruction> instructions)
        { 
            // Player
            //private void GetTotalFoodValue(out float hp, out float stamina)
            //  //hp = 25f;
            //	IL_0000: ldarg.1
            //**IL_0001: ldc.r4 25 // GetPlayerHealth()
            //  IL_0006: stind.r4
            //  //stamina = 75f;
            //  IL_0007: ldarg.2
            //**IL_0008: ldc.r4 75 // GetPlayerStamina()
            //  IL_000d: stind.r4
            instructions = PlayerHealthIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(25f)))
                .SetOperandAndAdvance(GetConfigPlayerHealth())
                .Log($"Modified Player.GetTotalFoodValue Base Health: {GetConfigPlayerHealth()}")
                .InstructionEnumeration();

            return PlayerStaminaIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(75f)))
                .SetOperandAndAdvance(GetConfigPlayerStamina())
                .Log($"Modified Player.GetTotalFoodValue Base Stamina: {GetConfigPlayerStamina()}")
                .InstructionEnumeration();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Player), "GetBaseFoodHP")]
        static IEnumerable<CodeInstruction> PatchGetBaseFoodHP(IEnumerable<CodeInstruction> instructions)
        {
            //instance float32 GetBaseFoodHP() cil manage
            //   // return 25f;
            //**IL_0000: ldc.r4 25 // GetPlayerHealth()
            return PlayerHealthIsUnaltered() ? instructions : new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Ldc_R4 && i.operand.Equals(25f)))
                .SetOperandAndAdvance(GetConfigPlayerHealth())
                .Log($"Modified Player.GetBaseFoodHP Base Health: {GetConfigPlayerHealth()}")
                .InstructionEnumeration();
        }

        static bool PlayerHealthIsUnaltered()
        {
            return GetConfigPlayerHealth() == 25f;
        }

        static float GetConfigPlayerHealth()
        {
            return ModifyPlayerHealthAndStamina.GetConfigPlayerHealth();
        }
        static bool PlayerStaminaIsUnaltered()
        {
            return GetConfigPlayerStamina() == 75f;
        }

        static float GetConfigPlayerStamina()
        {
            return ModifyPlayerHealthAndStamina.GetConfigPlayerStamina();
        }
    }
}
