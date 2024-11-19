using System;

namespace SpecialPowerUtilities.Models;

public class ModSectionData
{
    public string TabDisplayName;
    public string IconPath;
    public SourceRectData IconSourceRect;

    public Func<string> TabDisplayNameFunc = null;

    // Obsolete, only here for backwards compatibility.
    public string SectionName;
}