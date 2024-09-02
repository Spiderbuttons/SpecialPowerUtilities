using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ContentPatcher;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Sickhead.Engine.Util;
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
            Helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;

            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerUnavailable",
                PowerTriggerActions.SetPowerUnavailable);
            TriggerActionManager.RegisterAction("Spiderbuttons.SpecialPowerUtilities/Actions/SetPowerAvailable",
                PowerTriggerActions.SetPowerAvailable);
            GameStateQuery.Register("PLAYER_HAS_POWER", (string[] query, GameStateQueryContext ctx) =>
            {
                var powersData = DataLoader.Powers(Game1.content);
                return powersData.ContainsKey(query[2]) && GameStateQuery.CheckConditions(powersData[query[2]].UnlockedCondition, null, ctx.Player);
            });

            // harmony.Patch(original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.DailyLuck)),
            //     postfix: new HarmonyMethod(getMethod));

        }
        
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //Log.Warn(Assembly.GetExecutingAssembly().GetName());
            var type = Type.GetType("StardewValley.Game1");
            if (type is not null) Log.Warn(type.ToString());
            if (!Context.IsWorldReady) return;
            
            if (e.Button == SButton.F5)
            {
                Log.Info(RecipeBook.GetRecipeBooks().Count);
            }
        }

        public static DynamicMethod GetMethod(MethodBase method)
        {
            Log.Info("Testing!");
            Type returnType = typeof(void);

            Type othertype = Type.GetType("Game1");

            Type[] paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            Type declaringType = method.DeclaringType;
            
            if (method is MethodInfo info)
            {
                returnType = info.ReturnType;
                // select the TryParse method that takes a nullable string and returns an out bool
                var method2 = returnType.GetMethod("TryParse", new Type[] { typeof(string), returnType.MakeByRefType() });
                var methods = returnType.GetMethods().Where(m => m.Name == "TryParse" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters()[1].IsOut && m.GetParameters()[1].ParameterType == typeof(bool));
                if (method2 is not null) Log.Warn($"Method2: {method2.Name}: {method2.ToString()}");
                Log.Warn($"Return type: {returnType}");
                Log.Warn($"Param types: {string.Join(", ", paramTypes.Select(p => p.Name))}");
            }
            // var paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
            
                Log.Debug($"Method: {method.Name} DeclaringType: {method.DeclaringType}");
                DynamicMethod testMethod = new DynamicMethod($"{method.Name}_Patch", typeof(void), paramTypes, declaringType.Module);
                testMethod.DefineParameter(0, ParameterAttributes.Out, "__result");
                ILGenerator il = testMethod.GetILGenerator();
                var label = il.DefineLabel();
                il.Emit(OpCodes.Ldstr, "Hello, IL!");
                // call generic Log.Warn method with string argument
                il.Emit(OpCodes.Call,
                    AccessTools.Method(typeof(Log), nameof(Log.Warn), null, new Type[] { typeof(string) }));
                il.Emit(OpCodes.Ret);

                Log.Debug("Finished gen method");
            
                //testMethod.Invoke(null, null);
            
                return testMethod;
        }
        
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(6) && !DynamicPatcher.IsInitialized)
            {
                DynamicPatcher.Initialize(ModManifest);
                Helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            CP = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            CP?.RegisterToken(ModManifest, "HasPower", new HasPower());
            CP?.RegisterToken(ModManifest, "UnavailablePowers", new UnavailablePowers());
            
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
            
            if (e.NameWithoutLocale.IsEquivalentTo("Spiderbuttons.SpecialPowerUtilities/SimplePatches"))
            {
                e.LoadFrom(() => new List<SimpleDynamicPatch>(), AssetLoadPriority.Medium);
            }
        }
        
        private void OnMenuChange(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is not GameMenu menu || Config.UseVanillaMenu)
                return;
            int powersTabIndex = 0;
            for (int i = 0; i < menu.pages.Count; i++)
            {
                if (menu.pages[i] is not PowersTab && menu.pages[i] is not SPUTab)
                    continue;
                powersTabIndex = i;
                break;
            }
            Log.Trace($"Found PowersTab at index {powersTabIndex}");
            IClickableMenu oldTab = menu.pages[powersTabIndex];
            try
            {
                menu.pages[powersTabIndex] = new SPUTab(oldTab.xPositionOnScreen, oldTab.yPositionOnScreen, oldTab.width, oldTab.height);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to replace PowersTab: {ex.Message}");
            }
        }
    }
}