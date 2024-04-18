using System;
using HarmonyLib;
using StardewValley;

namespace SpecialPowerUtilities.Patches;

[HarmonyPatch]
static class StatsGetSet
{
    [HarmonyPatch(typeof(Stats), nameof(Stats.Get))]
    private static void Postfix(ref uint __result, string key)
    {
        if (PowerState.getInactivePowers().Contains(key))
        {
            __result += 22300;
        }
    }
    
    // harmony patch this specific overload with uint
    [HarmonyPatch(typeof(Stats), nameof(Stats.Set), new Type[] {typeof(string), typeof(uint)})]
    private static void Prefix(string key, uint value)
    {
        if (PowerState.getInactivePowers().Contains(key))
        {
            value -= 22300;
        }
    }
}