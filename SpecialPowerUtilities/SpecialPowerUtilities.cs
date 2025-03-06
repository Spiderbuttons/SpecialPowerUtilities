using ContentPatcher;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SpecialPowerUtilities.APIs;
using SpecialPowerUtilities.Config;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Menus;
using SpecialPowerUtilities.Models;
using SpecialPowerUtilities.Tokens;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Menus;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;

namespace SpecialPowerUtilities
{
    public class SpecialPowerUtilities : Mod
    {
        internal static IModHelper ModHelper { get; set; } = null!;
        internal static IMonitor ModMonitor { get; set; } = null!;

        internal static ModConfig Config { get; set; } = null!;
        internal static Harmony harmony { get; set; } = null!;

        internal static IContentPatcherAPI CP = null!;

        internal static IBetterGameMenuApi BetterGameMenu = null;

        internal static IUnlockableBundlesAPI UBundles = null!;

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
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;

            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerUnavailable",
                PowerTriggerActions.SetPowerUnavailable);
            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerAvailable",
                PowerTriggerActions.SetPowerAvailable);
            GameStateQuery.Register("PLAYER_HAS_POWER", (string[] query, GameStateQueryContext ctx) =>
            {
                var powersData = DataLoader.Powers(Game1.content);
                return powersData.ContainsKey(query[2]) &&
                       GameStateQuery.CheckConditions(powersData[query[2]].UnlockedCondition, null, ctx.Player);
            });
        }

        public override object GetApi()
        {
            return new SpecialPowerAPI();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CP = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            CP?.RegisterToken(ModManifest, "HasPower", new HasPower());
            CP?.RegisterToken(ModManifest, "UnavailablePowers", new UnavailablePowers());

            UBundles = Helper.ModRegistry.GetApi<IUnlockableBundlesAPI>("DLX.Bundles");

            BetterGameMenu = Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
            if (BetterGameMenu is not null)
            {
                BetterGameMenu.RegisterImplementation(
                    nameof(VanillaTabOrders.Powers),
                    priority: 100,
                    getPageInstance: menu =>
                        new SPUTab(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height),
                    getWidth: width => width - 64 - 16,
                    onResize: input => new SPUTab(input.Menu.xPositionOnScreen, input.Menu.yPositionOnScreen,
                        input.Menu.width, input.Menu.height)
                );
            }

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null) Config.SetupConfig(configMenu, ModManifest, Helper);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            RecipeBook.GrantRecipesAgain();
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Spiderbuttons.SpecialPowerUtilities/PowerTabs") ||
                e.NameWithoutLocale.IsEquivalentTo("Spiderbuttons.SpecialPowerUtilities/PowerSections"))
            {
                e.LoadFrom(() => new Dictionary<string, ModSectionData>(), AssetLoadPriority.High);
            }

            if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Book_Animation") && Config.StripelessBooks)
            {
                e.LoadFromModFile<Texture2D>("Assets/Book_Animation.png", AssetLoadPriority.Medium);
            }
        }

        private void OnMenuChange(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not GameMenu menu || Config.UseVanillaMenu)
                return;
            
            int powersTabIndex = -1;
            for (int i = 0; i < menu.pages.Count; i++)
            {
                if (menu.pages[i] is not PowersTab && menu.pages[i] is not SPUTab)
                    continue;
                powersTabIndex = i;
                break;
            }

            if (powersTabIndex < 0)
            {
                Log.Warn("Failed to find Powers tab in GameMenu. This is likely due to another mod modifying GameMenu.");
                return;
            }
            
            Log.Trace($"Found PowersTab at index {powersTabIndex}");
            IClickableMenu oldTab = menu.pages[powersTabIndex];
            try
            {
                menu.pages[powersTabIndex] = new SPUTab(oldTab.xPositionOnScreen, oldTab.yPositionOnScreen,
                    oldTab.width, oldTab.height);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to replace PowersTab: {ex.Message}");
            }
        }
    }
}