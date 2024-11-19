using System;
using Microsoft.Xna.Framework;
using SpecialPowerUtilities.Menus;
using SpecialPowerUtilities.Models;

namespace SpecialPowerUtilities.APIs;

public class SpecialPowerAPI : ISpecialPowerAPI
{
    public bool RegisterPowerCategory(string uniqueID, Func<string> displayName, string iconPath, Point sourceRectPosition = default,
        Point sourceRectSize = default)
    {
        if (SPUTab.registeredTabs.ContainsKey(uniqueID))
            return false;
        
        SPUTab.registeredTabs.Add(uniqueID, new ModSectionData
        {
            TabDisplayName = displayName(),
            TabDisplayNameFunc = displayName,
            IconPath = iconPath,
            IconSourceRect = new SourceRectData
            {
                X = sourceRectPosition.X,
                Y = sourceRectPosition.Y,
                Width = sourceRectSize.X,
                Height = sourceRectSize.Y
            }
        });
        
        return true;
    }
}