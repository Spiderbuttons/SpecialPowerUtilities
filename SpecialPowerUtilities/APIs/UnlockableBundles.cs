using StardewValley;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SpecialPowerUtilities.APIs
{
    public interface IUnlockableBundlesAPI
    {
        /// <summary> Returns the wallet currency value of a player </summary>
        /// <param name="currencyId"></param>
        /// <param name="who">The players unique multiplayer id</param>
        int getWalletCurrency(string currencyId, long who);
    }
}