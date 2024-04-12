using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Models;
using StardewValley.Menus;
using StardewValley.GameData.Powers;
using StardewValley.TokenizableStrings;
using Object = System.Object;

namespace SpecialPowerUtilities.Menus
{
    public class SPUTab : IClickableMenu
    {
        public static readonly string[] vanillaPowerNames = new string[]
        {
            "ForestMagic", "DwarvishTranslationGuide", "RustyKey", "ClubCard", "SpecialCharm", "SkullKey",
            "MagnifyingGlass", "DarkTalisman", "MagicInk", "BearPaw", "SpringOnionMastery", "KeyToTheTown",
            "Book_PriceCatalogue", "Book_Marlon", "Book_Speed", "Book_Speed2", "Book_Void", "Book_Friendship",
            "Book_Defense", "Book_Woodcutting", "Book_WildSeeds", "Book_Roe", "Book_Bombs", "Book_Crabbing",
            "Book_Trash", "Book_Diamonds", "Book_Mystery", "Book_Horse", "Book_Artifact", "Book_Grass",
            "Book_AnimalCatalogue", "Mastery_Farming", "Mastery_Fishing", "Mastery_Foraging", "Mastery_Mining",
            "Mastery_Combat"
        };

        public static int modID = 21526;

        public int region_forwardButton = modID + 1;

        public int region_backButton = modID + 2;

        public  int distanceFromMenuBottomBeforeNewPage = 128;

        public int widthToMoveActiveTab = 8;

        public ClickableTextureComponent backButton;

        public ClickableTextureComponent forwardButton;

        public Dictionary<string, ClickableTextureComponent> sideTabs = new Dictionary<string, ClickableTextureComponent>();
        
        public Dictionary<string, List<List<ClickableTextureComponent>>> categories = new Dictionary<string, List<List<ClickableTextureComponent>>>();
        
        public Dictionary<string, CategoryData> categoryData = new Dictionary<string, CategoryData>();

        public string currentTab = "Stardew Valley";

        public int currentPage;

        private string descriptionText = "";

        private string hoverText = "";
        
        Dictionary<string, SPUData> allPowers = new Dictionary<string, SPUData>();
        
        Texture2D StardewIcon = ModEntry.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/StardewPowers.png");
        Texture2D ModdedIcon = ModEntry.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/ModdedPowers.png");
        
        IModHelper Helper = ModEntry.ModHelper;

        public SPUTab(int x, int y, int width, int height) : base(x, y, width, height)
        {
            this.backButton = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + height - 80, 48, 44),
                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = region_backButton,
                rightNeighborID = -7777
            };
            this.forwardButton = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen + width - 32 - 60, base.yPositionOnScreen + height - 80, 48, 44),
                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = region_forwardButton,
                leftNeighborID = -7777
            };
            this.sideTabs.Add("Stardew Valley", new ClickableTextureComponent(0.ToString() ?? "",
                new Rectangle(base.xPositionOnScreen - 48 + widthToMoveActiveTab,
                    base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", "Stardew Valley", StardewIcon,
                new Rectangle(0, 0, 32, 32), 2f)
            {
                myID = sideTabs.Count,
                downNeighborID = -99998,
                rightNeighborID = 0
            });
            this.categories.Add("Stardew Valley", new List<List<ClickableTextureComponent>>());
            
            loadMods();
            separateMods();
            
            // move the "Miscellaneous" category to the end but only if it actually exists
            if (categories.ContainsKey("Miscellaneous"))
            {
                List<List<ClickableTextureComponent>> misc = categories["Miscellaneous"];
                categories.Remove("Miscellaneous");
                categories.Add("Miscellaneous", misc);
            }
            
            foreach (var tab in categories)
            {
                if (tab.Key == "Stardew Valley") continue;
                if (tab.Key == "Miscellaneous")
                {
                    this.sideTabs.Add(tab.Key, new ClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 48,
                            base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", tab.Key,
                        ModdedIcon,
                        new Rectangle(0, 0, 32, 32), 2f)
                    {
                        myID = sideTabs.Count,
                        downNeighborID = -99998,
                        rightNeighborID = 0
                    });
                }
                else
                {
                    Texture2D icon;
                    icon = categoryData[tab.Key].IconPath != null ? Game1.content.Load<Texture2D>(categoryData[tab.Key].IconPath) : ModdedIcon;
                    
                    this.sideTabs.Add(tab.Key, new ClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 48,
                            base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", categoryData[tab.Key].CategoryName,
                        icon,
                        new Rectangle(categoryData[tab.Key].IconPosition.X, categoryData[tab.Key].IconPosition.Y, 32, 32), 2f)
                    {
                        myID = sideTabs.Count,
                        downNeighborID = -99998,
                        rightNeighborID = 0
                    });
                
                }
            }
            
            this.sideTabs["Stardew Valley"].upNeighborID = -1;
            this.sideTabs["Stardew Valley"].upNeighborImmutable = true;
            
            string last_tab = "Stardew Valley";
            int last_y = 0;
            foreach (string key in this.sideTabs.Keys)
            {
                if (this.sideTabs[key].bounds.Y > last_y)
                {
                    last_y = this.sideTabs[key].bounds.Y;
                    last_tab = key;
                }
            }
            
            this.sideTabs[last_tab].downNeighborID = -1;
            this.sideTabs[last_tab].downNeighborImmutable = true;
        }

        public override void snapToDefaultClickableComponent()
        {
            base.snapToDefaultClickableComponent();
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }

        public SPUData convertToSPU(PowersData powersData)
        {
            SPUData spuData = new SPUData();
            spuData.DisplayName = powersData.DisplayName;
            spuData.Description = powersData.Description;
            spuData.TexturePath = powersData.TexturePath;
            spuData.TexturePosition = powersData.TexturePosition;
            spuData.UnlockedCondition = powersData.UnlockedCondition;
            
            return spuData;
        }

        public void loadMods()
        {
            try
            {
                var powersData = DataLoader.Powers(Game1.content);
                foreach (KeyValuePair<string, PowersData> power in powersData)
                {
                    SPUData spuData = convertToSPU(power.Value);
                    allPowers.Add(power.Key, spuData);
                }
            }
            catch (Exception ex)
            {
                Loggers.Log("Failed to load powers data: " + ex.Message);
            }
            
            var cats = Game1.content.Load<Dictionary<string, CategoryData>>("SpecialPowerUtilities/Categories");
            foreach (var cat in cats)
            {
                if (!categories.ContainsKey(cat.Value.CategoryName))
                {
                    categories.Add(cat.Value.CategoryName, new List<List<ClickableTextureComponent>>());
                    categoryData.Add(cat.Value.CategoryName, cat.Value);
                }
            }
            var powers = Game1.content.Load<Dictionary<string, SPUData>>("SpecialPowerUtilities/Powers");
            foreach (var power in powers)
            {
                if (allPowers.TryGetValue(power.Key, out var allPower))
                {
                    allPower.Category = power.Value.Category;
                }
            }
        }

        public void separateMods()
        {
            if (allPowers == null || allPowers.Count == 0) return;
            
            int categoryWidth = 9;
            Dictionary<string, int> widthUsed = new Dictionary<string, int>();
            int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
            int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;

            foreach (KeyValuePair<string, SPUData> power in allPowers)
            {
                string whichCategory;
                if (vanillaPowerNames.Contains(power.Key)) whichCategory = "Stardew Valley";
                else
                {
                    whichCategory = power.Value.Category ?? Utils.TryGetModFromString(power.Key)?.Manifest.UniqueID.Split(".")[1] ?? "Miscellaneous";
                }

                if (!categories.ContainsKey(whichCategory))
                {
                    categories.Add(whichCategory, new List<List<ClickableTextureComponent>>());
                    categoryData.Add(whichCategory, new CategoryData()
                    {
                        CategoryName = whichCategory,
                        IconPath = null,
                        IconPosition = new Point(0, 0)
                    });
                }
                if (!widthUsed.ContainsKey(whichCategory)) widthUsed.Add(whichCategory, 0);
                int xPos = baseX + widthUsed[whichCategory] % categoryWidth * 76;
                int yPos = baseY + widthUsed[whichCategory] / categoryWidth * 76; ;
                if (yPos > base.yPositionOnScreen + height + distanceFromMenuBottomBeforeNewPage)
                {
                    categories[whichCategory].Add(new List<ClickableTextureComponent>());
                    widthUsed[whichCategory] = 0;
                    xPos = baseX;
                    yPos = baseY;
                }

                if (!categories.ContainsKey(whichCategory))
                {
                    categories.Add(whichCategory, new List<List<ClickableTextureComponent>>());
                }
                if (categories[whichCategory].Count == 0)
                {
                    categories[whichCategory].Add(new List<ClickableTextureComponent>());
                }
                List<ClickableTextureComponent> list = categories[whichCategory].Last();
                bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
                string name = TokenParser.ParseText(power.Value.DisplayName);
                string description = TokenParser.ParseText(power.Value.Description);
                Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
                list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null, description, texture, new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f, unlocked)
                {
                    myID = list.Count,
                    rightNeighborID = (((list.Count + 1) % categoryWidth == 0) ? (-1) : (list.Count + 1)),
                    leftNeighborID = ((list.Count % categoryWidth == 0) ? (-1) : (list.Count - 1)),
                    downNeighborID = ((yPos + 76 > base.yPositionOnScreen + height + distanceFromMenuBottomBeforeNewPage) ? (-7777) : (list.Count + categoryWidth)),
                    upNeighborID = ((list.Count < categoryWidth) ? 12346 : (list.Count - categoryWidth)),
                    fullyImmutable = true
                });
                widthUsed[whichCategory]++;
            }
        }

        public override void populateClickableComponentList()
        {
            // if (this.powers == null)
            // {
            //     this.powers = new List<List<ClickableTextureComponent>>();
            //     Dictionary<string, PowersData> powersData = null;
            //     Dictionary<string, SPUData> SPUData = new Dictionary<string, SPUData>();
            //     try
            //     {
            //         powersData = DataLoader.Powers(Game1.content);
            //     }
            //     catch (Exception ex)
            //     {
            //         Loggers.Log("Failed to load powers data: " + ex.Message);
            //     }
            //
            //     // convert all the powers in PowersData to SPUData objects
            //     foreach (KeyValuePair<string, PowersData> power in powersData)
            //     {
            //         SPUData spuData = new SPUData(power.Value);
            //         SPUData.Add(power.Key, spuData);
            //     }
            //
            //     if (SPUData != null)
            //     {
            //         int collectionWidth = 9;
            //         int widthUsed = 0;
            //         int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth +
            //                     IClickableMenu.spaceToClearSideBorder;
            //         int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth +
            //             IClickableMenu.spaceToClearTopBorder - 16;
            //         foreach (KeyValuePair<string, SPUData> power in SPUData)
            //         {
            //             int xPos = baseX + widthUsed % collectionWidth * 76;
            //             int yPos = baseY + widthUsed / collectionWidth * 76;
            //             bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
            //             string name = TokenParser.ParseText(power.Value.DisplayName);
            //             string description = TokenParser.ParseText(power.Value.Description);
            //             Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
            //             string category = TokenParser.ParseText(power.Value.Category);
            //             if (this.powers.Count == 0 || yPos > base.yPositionOnScreen + base.height - 128)
            //             {
            //                 this.powers.Add(new List<ClickableTextureComponent>());
            //                 widthUsed = 0;
            //                 xPos = baseX;
            //                 yPos = baseY;
            //             }
            //
            //             List<ClickableTextureComponent> list = this.powers.Last();
            //             list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null,
            //                 description, texture,
            //                 new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f,
            //                 unlocked)
            //             {
            //                 myID = list.Count,
            //                 rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
            //                 leftNeighborID = ((list.Count % collectionWidth == 0) ? (-1) : (list.Count - 1)),
            //                 downNeighborID = ((yPos + 76 > base.yPositionOnScreen + base.height - 128)
            //                     ? (-7777)
            //                     : (list.Count + collectionWidth)),
            //                 upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
            //                 fullyImmutable = true
            //             });
            //             widthUsed++;
            //         }
            //     }
            // }

            base.populateClickableComponentList();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (KeyValuePair<string, ClickableTextureComponent> v in sideTabs)
            {
                if (v.Value.containsPoint(x, y) && currentTab != v.Key)
                {
                    Game1.playSound("smallSelect");
                    sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
                    currentTab = v.Key;
                    currentPage = 0;
                    v.Value.bounds.X += widthToMoveActiveTab;
                }
            }

            if (currentPage > 0 && backButton.containsPoint(x, y))
            {
                currentPage--;
                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == 0)
                {
                    base.currentlySnappedComponent = forwardButton;
                    Game1.setMousePosition(base.currentlySnappedComponent.bounds.Center);
                }
            }

            if (currentPage < categories[currentTab].Count - 1 && forwardButton.containsPoint(x, y))
            {
                currentPage++;
                Game1.playSound("shwip");
                forwardButton.scale = forwardButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls &&
                    currentPage == categories[currentTab].Count - 1)
                {
                    base.currentlySnappedComponent = backButton;
                    Game1.setMousePosition(base.currentlySnappedComponent.bounds.Center);
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
            this.descriptionText = "";
            base.performHoverAction(x, y);
            foreach (ClickableTextureComponent tab in this.sideTabs.Values)
            {
                if (tab.containsPoint(x, y))
                {
                    this.hoverText = tab.hoverText;
                    return;
                }
            }

            foreach (ClickableTextureComponent c in categories[currentTab][currentPage])
            {
                if (c.containsPoint(x, y))
                {
                    c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
                    this.hoverText = (c.drawShadow ? c.name : "???");
                    this.descriptionText = Game1.parseText(c.hoverText, Game1.smallFont,
                        Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverText).X, 320));
                }
                else
                {
                    c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
                }
            }

            this.forwardButton.tryHover(x, y, 0.5f);
            this.backButton.tryHover(x, y, 0.5f);
        }

        public override void draw(SpriteBatch b)
        {
            foreach (ClickableTextureComponent value in this.sideTabs.Values)
            {
                value.draw(b);
            }

            if (this.currentPage > 0)
            {
                this.backButton.draw(b);
            }

            if (this.currentPage < categories[currentTab].Count - 1)
            {
                this.forwardButton.draw(b);
            }

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (ClickableTextureComponent item in categories[currentTab][currentPage])
            {
                bool drawColor = item.drawShadow;
                item.draw(b, drawColor ? Color.White : (Color.Black * 0.2f), 0.86f);
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (!this.descriptionText.Equals("") && this.hoverText != "???")
            {
                IClickableMenu.drawHoverText(b, this.descriptionText, Game1.smallFont, 0, 0, -1, this.hoverText);
            }
            else if (!this.hoverText.Equals(""))
            {
                IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
            }
        }
    }
}