using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace SpecialPowerUtilities;

public static class PowerState
{
    private static List<string> activePowers;
    private static List<string> inactivePowers;

    static PowerState()
    {
        inactivePowers = getInactivePowers();
    }

    public static List<string> getInactivePowers()
    {
        if (Game1.player.modData.TryGetValue("Spiderbuttons.SpecialPowerUtilities/InactivePowers", out var modData))
        {
            return modData.Split(',').ToList();
        }
        return new List<string>();
    }
    
    public static void deactivatePower(Farmer player, string power)
    {
        if (inactivePowers.Contains(power)) return;
        inactivePowers.Add(power);
        player.modData["Spiderbuttons.SpecialPowerUtilities/InactivePowers"] = string.Join(",", inactivePowers);
    }
    
    public static void activatePower(Farmer player, string power)
    {
        if (!inactivePowers.Contains(power)) return;
        inactivePowers.Remove(power);
        player.modData["Spiderbuttons.SpecialPowerUtilities/InactivePowers"] = string.Join(",", inactivePowers);
    }
}