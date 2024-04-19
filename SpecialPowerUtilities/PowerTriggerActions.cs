using System;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace SpecialPowerUtilities;

public class PowerTriggerActions
{
    public static bool SetPowerUnavailable(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerUnavailable <who> <powerID>

        if (!ArgUtility.TryGet(args, 1, out var who, out error) || !ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            return false;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.setPowerUnavailable(target, powerID);
            }
            catch (Exception ex)
            {
                Loggers.Log("Error in SetPowerUnavailable: " + ex.Message, LogLevel.Error);
                return false;
            }

            return true;
        });
        return success;
    }
    
    public static bool SetPowerAvailable(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerAvailable <who> <powerID>

        if (!ArgUtility.TryGet(args, 1, out var who, out error) || !ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            return false;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.setPowerAvailable(target, powerID);
            }
            catch (Exception ex)
            {
                Loggers.Log("Error in SetPowerAvailable: " + ex.Message, LogLevel.Error);
                return false;
            }

            return true;
        });
        return success;
    }
}