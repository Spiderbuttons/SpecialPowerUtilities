using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Constants;
using StardewValley.GameData.Powers;

namespace SpecialPowerUtilities.Tokens;

internal class HasPower
{
    private List<string> currentPlayerPowers;
    private List<string> allPlayerPowers;

    public HasPower()
    {
        var powers = GetPowers();
        currentPlayerPowers = powers[0];
        allPlayerPowers = powers[1];
    }
    
    private List<List<string>> GetPowers()
    {
        if (currentPlayerPowers == null && allPlayerPowers == null)
        {
            currentPlayerPowers = new List<string>();
            allPlayerPowers = new List<string>();
            return new List<List<string>> {currentPlayerPowers, allPlayerPowers};
        }
        var powersData = Game1.content.Load<Dictionary<string, PowersData>>("Data/Powers");
        var farmers = Game1.getAllFarmers().ToList();
        foreach (var power in powersData)
        {
            foreach (var farmer in farmers.Where(farmer => GameStateQuery.CheckConditions(power.Value.UnlockedCondition, null, farmer)))
            {
                if (!allPlayerPowers.Contains(power.Key))
                {
                    allPlayerPowers.Add(power.Key);
                }
                if (farmer == Game1.player)
                {
                    currentPlayerPowers?.Add(power.Key);
                }
            }
        }
        return new List<List<string>> {currentPlayerPowers, allPlayerPowers};
    }
    
    public bool AllowsInput()
    {
        return true;
    }

    public bool RequiresInput()
    {
        return true;
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
        var powers = GetPowers();
        currentPlayerPowers = powers[0];
        allPlayerPowers = powers[1];
        return !currentPlayerPowers.SequenceEqual(oldCurrentPlayerPowers) || !allPlayerPowers.SequenceEqual(oldAllPlayerPowers);
    }

    public bool IsReady()
    {
        if (Context.IsWorldReady || SaveGame.loaded?.player != null) return true;
        return false;
    }

    public IEnumerable<string> GetValues(string? input)
    {
        if (input == null) yield break;
        string farmerToCheck = input.Split(' ')[0];
        string powerToCheck = input.Split(' ')[1];
        if (farmerToCheck == "Current")
        {
            if (currentPlayerPowers.Contains(powerToCheck))
            {
                yield return "true";
            }
        }
        else if (farmerToCheck == "Any")
        {
            if (allPlayerPowers.Contains(powerToCheck))
            {
                yield return "true";
            }
        }
        
        yield return "false";
    }
}