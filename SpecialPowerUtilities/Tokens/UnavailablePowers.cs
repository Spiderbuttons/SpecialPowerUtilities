using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.GameData.Powers;

namespace SpecialPowerUtilities.Tokens;

internal class UnavailablePowers
{
    private List<string> currentPlayerPowers;
    private List<string> allPlayerPowers;

    public UnavailablePowers()
    {
        var powers = GetUnavailablePowers();
        currentPlayerPowers = powers[0];
        allPlayerPowers = powers[1];
    }

    private List<List<string>> GetUnavailablePowers()
    {
        if (currentPlayerPowers == null && allPlayerPowers == null)
        {
            currentPlayerPowers = new List<string>();
            allPlayerPowers = new List<string>();
            return new List<List<string>> { currentPlayerPowers, allPlayerPowers };
        }

        var farmers = Game1.getAllFarmers().ToList();
        foreach (var farmer in farmers)
        {
            var unavailablePowers = PowerState.getUnavailablePowers(farmer);
            allPlayerPowers.AddRange(unavailablePowers);
            if (farmer == Game1.player)
            {
                currentPlayerPowers?.AddRange(unavailablePowers);
            }
        }

        return new List<List<string>> { currentPlayerPowers, allPlayerPowers };
    }

    public bool AllowsInput()
    {
        return true;
    }

    public bool RequiresInput()
    {
        return false;
    }

    public bool CanHaveMultipleValues(string input = null)
    {
        return false;
    }

    public bool UpdateContext()
    {
        List<string> oldCurrentPlayerPowers = new(currentPlayerPowers);
        List<string> oldAllPlayerPowers = new(allPlayerPowers);
        currentPlayerPowers.Clear();
        allPlayerPowers.Clear();
        var powers = GetUnavailablePowers();
        currentPlayerPowers = powers[0];
        allPlayerPowers = powers[1];
        return !currentPlayerPowers.SequenceEqual(oldCurrentPlayerPowers) ||
               !allPlayerPowers.SequenceEqual(oldAllPlayerPowers);
    }

    public bool IsReady()
    {
        if (Context.IsWorldReady || SaveGame.loaded?.player != null) return true;
        return false;
    }

    public IEnumerable<string> GetValues(string input)
    {
        string farmerToCheck = input?.Split(' ')[0];
        if (farmerToCheck is null or "Current")
        {
            foreach (var power in currentPlayerPowers)
            {
                yield return power;
            }
        }
        else if (farmerToCheck == "Any")
        {
            foreach (var power in allPlayerPowers)
            {
                yield return power;
            }
        }

        yield return null;
    }
}