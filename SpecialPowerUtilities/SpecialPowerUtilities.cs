using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ContentPatcher;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using HarmonyLib;
using SpecialPowerUtilities.Config;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Menus;
using SpecialPowerUtilities.Models;
using SpecialPowerUtilities.Tokens;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;
using StardewValley.Triggers;

namespace SpecialPowerUtilities
{
    public class SpecialPowerUtilities : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;
        
        internal static ModConfig Config { get; set; } = null!;
        internal static Harmony harmony { get; set; } = null!;
        
        internal static IContentPatcherAPI CP = null!;
        
        public override void Entry(IModHelper helper)
        {
            i18n.Init(helper.Translation);
            ModHelper = helper;
            ModMonitor = Monitor;
            Config = helper.ReadConfig<ModConfig>();
            harmony = new Harmony(ModManifest.UniqueID);
            
            harmony.PatchAll();
            Helper.Events.Display.MenuChanged += OnMenuChange;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/SetPowerUnavailable",
                PowerTriggerActions.SetPowerUnavailable);
            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/SetPowerAvailable",
                PowerTriggerActions.SetPowerAvailable);
        }
        
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CP = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            CP?.RegisterToken(ModManifest, "HasPower", new HasPower());
            CP?.RegisterToken(ModManifest, "UnavailablePowers", new UnavailablePowers());
            
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.SetupConfig(configMenu, ModManifest, Helper);
        }
        
        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            Helper.GameContent.InvalidateCache("SpecialPowerUtilities/Powers");
            if (e.NameWithoutLocale.IsEquivalentTo("Spiderbuttons.SpecialPowerUtilities/PowerSections"))
                e.LoadFrom(() => new Dictionary<string, ModSectionData>(), AssetLoadPriority.High);
        }
        
        private void OnMenuChange(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not GameMenu menu)
                return;
            int powersTabIndex = 0;
            for (int i = 0; i < menu.pages.Count; i++)
            {
                if (menu.pages[i] is not PowersTab powersTab)
                    continue;
                powersTabIndex = i;
                break;
            }
            Loggers.Log($"Found PowersTab at index {powersTabIndex}", LogLevel.Trace);
            IClickableMenu oldTab = menu.pages[powersTabIndex];
            try
            {
                menu.pages[powersTabIndex] = new SPUTab(oldTab.xPositionOnScreen, oldTab.yPositionOnScreen, oldTab.width, oldTab.height);
            }
            catch (Exception ex)
            {
                Loggers.Log($"Failed to replace PowersTab: {ex.Message}", LogLevel.Error);
            }
        }
    }
}