using System;
using Microsoft.Xna.Framework;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Menus;
using SpecialPowerUtilities.Models;

namespace SpecialPowerUtilities.APIs;

public class SpecialPowerAPI : ISpecialPowerAPI
{
    public bool RegisterPowerCategory(string uniqueID, Func<string> displayName, string iconTexture)
    {
        if (SPUTab.registeredTabs.ContainsKey(uniqueID))
            return false;

        try
        {
            SPUTab.registeredTabs.Add(uniqueID, new ModSectionData
            {
                TabDisplayName = displayName(),
                TabDisplayNameFunc = displayName,
                IconPath = iconTexture,
            });
            return true;
        } catch (Exception e)
        {
            Log.Error("Error registering power category: " + e);
            return false;
        }
    }
    
    public bool RegisterPowerCategory(string uniqueID, Func<string> displayName, string iconTexture, Point sourceRectPosition,
        Point sourceRectSize)
    {
        if (SPUTab.registeredTabs.ContainsKey(uniqueID))
            return false;

        try
        {
            SPUTab.registeredTabs.Add(uniqueID, new ModSectionData
            {
                TabDisplayName = displayName(),
                TabDisplayNameFunc = displayName,
                IconPath = iconTexture,
                IconSourceRect = new SourceRectData
                {
                    X = sourceRectPosition.X,
                    Y = sourceRectPosition.Y,
                    Width = sourceRectSize.X,
                    Height = sourceRectSize.Y
                }
            });
            return true;
        } catch (Exception e)
        {
            Log.Error("Error registering power category: " + e);
            return false;
        }
    }
}