using System;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace SpecialPowerUtilities;

public class PowerTriggerActions
{
    public static bool SetPowerInactive(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerInactive <who> <powerID>

        if (!ArgUtility.TryGet(args, 1, out var who, out error) || !ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            return false;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.deactivatePower(target, powerID);
            }
            catch (Exception ex)
            {
                Loggers.Log("Error in SetPowerInactive: " + ex.Message, LogLevel.Error);
                return false;
            }

            return true;
        });
        return success;
    }
    
    public static bool SetPowerActive(string[] args, TriggerActionContext context, out string error)
    {
        //SetPowerActive <who> <powerID>

        if (!ArgUtility.TryGet(args, 1, out var who, out error) || !ArgUtility.TryGet(args, 2, out var powerID, out error))
        {
            return false;
        }
        
        bool success = GameStateQuery.Helpers.WithPlayer(Game1.player, who, target =>
        {
            try
            {
                PowerState.activatePower(target, powerID);
            }
            catch (Exception ex)
            {
                Loggers.Log("Error in SetPowerActive: " + ex.Message, LogLevel.Error);
                return false;
            }

            return true;
        });
        return success;
    }
}