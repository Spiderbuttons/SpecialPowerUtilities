using StardewModdingAPI;

namespace SpecialPowerUtilities.Helpers;

public static class Utils
{
    public static IModInfo? TryGetModFromString(string id)
    {
        string[] parts = id.Split('_');
        if (parts.Length == 1) return null;

        string modId = parts[0];
        int idIndex = parts.Length - 1;
        for (int i = 0; i < idIndex; i++)
        {
            if (i != 0) modId += '_' + parts[i];

            IModInfo? mod = SpecialPowerUtilities.ModHelper.ModRegistry.Get(modId);
            if (mod != null) return mod;
        }

        return null;
    }
}