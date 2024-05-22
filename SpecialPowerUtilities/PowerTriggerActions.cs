using System;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Minigames;

namespace SpecialPowerUtilities;

public class PowerTriggerActions
{
    public static bool SetPowerUnavailable(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerUnavailable [who] <powerID>
        if (!ArgUtility.TryGet(args, 1, out var who, out error))
        {
            return false;
        }
        if (!ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            powerID = who;
            who = "Current";
            error = null;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.setPowerUnavailable(target, powerID);
            }
            catch (Exception ex)
            {
                Log.Error("Error in SetPowerUnavailable: " + ex.Message);
                return false;
            }
        
            return true;
        });
        return success;
    }
    
    public static bool SetPowerAvailable(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerAvailable [who] <powerID>
        if (!ArgUtility.TryGet(args, 1, out var who, out error))
        {
            return false;
        }
        
        if (!ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            powerID = who;
            who = "Current";
            error = null;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.setPowerAvailable(target, powerID);
            }
            catch (Exception ex)
            {
                Log.Error("Error in SetPowerAvailable: " + ex.Message);
                return false;
            }
            return true;
        });
        return success;
    }
}