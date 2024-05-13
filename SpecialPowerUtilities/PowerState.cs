using System.Collections.Generic;
using System.Linq;
using StardewValley;

namespace SpecialPowerUtilities;

public static class PowerState
{ 
    public static List<string> getUnavailablePowers(Farmer who)
    {
        if (who.modData.TryGetValue("Spiderbuttons.SpecialPowerUtilities/Powers/UnavailablePowers", out var modData))
        {
            return modData.Split(',').ToList();
        }
        return new List<string>();
    }
    
    public static void setPowerUnavailable(Farmer player, string power)
    {
        var unavailablePowers = getUnavailablePowers(player);
        if (unavailablePowers.Contains(power)) return;
        unavailablePowers.Add(power);
        player.modData["Spiderbuttons.SpecialPowerUtilities/Powers/UnavailablePowers"] = string.Join(",", unavailablePowers);
    }
    
    public static void setPowerAvailable(Farmer player, string power)
    {
        var unavailablePowers = getUnavailablePowers(player);
        if (!unavailablePowers.Contains(power)) return;
        unavailablePowers.Remove(power);
        player.modData["Spiderbuttons.SpecialPowerUtilities/Powers/UnavailablePowers"] = string.Join(",", unavailablePowers);
    }
}