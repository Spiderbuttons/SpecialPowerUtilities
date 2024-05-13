using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;

namespace SpecialPowerUtilities.Helpers;

public static class Loggers
{
    public static void Log(string message, LogLevel level = LogLevel.Debug) => SpecialPowerUtilities.ModMonitor.Log(message, level);
    
    public static void ILCode(List<CodeInstruction> code)
    {
        for (var i = 0; i < code.Count; i++)
        {
            Log($"{i}: {code[i].opcode} {code[i].operand}", LogLevel.Debug);
        }
    }
    
    public static void ILCode(CodeInstruction code)
    {
        Log($"{code.opcode} {code.operand}", LogLevel.Debug);
    }
}