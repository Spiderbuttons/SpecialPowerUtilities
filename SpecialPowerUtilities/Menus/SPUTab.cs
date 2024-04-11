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

        public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();
        
        public Dictionary<string, List<List<ClickableTextureComponent>>> categories = new Dictionary<string, List<List<ClickableTextureComponent>>>();

        public int currentTab;

        public int currentPage;

        private string descriptionText = "";

        private string hoverText = "";

        public List<List<ClickableTextureComponent>> vanillaPowers;
        
        public List<KeyValuePair<string, SPUData>> moddedPowers;

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
            this.sideTabs.Add(0, new ClickableTextureComponent(0.ToString() ?? "",
                new Rectangle(base.xPositionOnScreen - 48 + CollectionsPage.widthToMoveActiveTab,
                    base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", "Stardew Valley", Game1.mouseCursors,
                new Rectangle(640, 80, 16, 16), 4f)
            {
                myID = 1,
                downNeighborID = -99998,
                rightNeighborID = 0
            });
            this.categories.Add("Stardew Valley", new List<List<ClickableTextureComponent>>());
            
            this.sideTabs[0].upNeighborID = -1;
            this.sideTabs[0].upNeighborImmutable = true;
            int last_tab = 0;
            int last_y = 0;
            foreach (int key in this.sideTabs.Keys)
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

        public void getPowers()
        { 
            var moddedPowers = new List<List<ClickableTextureComponent>>();
            Dictionary<string, SPUData> SPUData = null;
            try
            {
                SPUData = ModEntry.ModHelper.GameContent.Load<Dictionary<string, SPUData>>("Mods\\SPU\\PowerExtensions");
            }
            catch
            {
                Loggers.Log("Failed to load modded powers data");
                return;
            }
        }

        public void populateModdedPowers()
        {
            if (this.moddedPowers != null)
            {
                // make a mods category if there isnt one already
                if (!this.categories.ContainsKey("Mods"))
                {
                    this.categories.Add("Mods", new List<List<ClickableTextureComponent>>());
                    this.sideTabs.Add(this.sideTabs.Count, new ClickableTextureComponent(this.sideTabs.Count.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 48 + CollectionsPage.widthToMoveActiveTab,
                            base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", "Mods", Game1.mouseCursors,
                        new Rectangle(640, 80, 16, 16), 4f)
                    {
                        myID = this.sideTabs.Count,
                        downNeighborID = -99998,
                        rightNeighborID = this.sideTabs.Count - 1
                    });
                    this.sideTabs[this.sideTabs.Count - 1].upNeighborID = -1;
                    this.sideTabs[this.sideTabs.Count - 1].upNeighborImmutable = true;
                    int last_tab = 0;
                    int last_y = 0;
                    foreach (int key in this.sideTabs.Keys)
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
                int collectionWidth = 9;
                int widthUsed = 0;
                int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth +
                            IClickableMenu.spaceToClearSideBorder;
                int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth +
                    IClickableMenu.spaceToClearTopBorder - 16;
                foreach (KeyValuePair<string, SPUData> power in moddedPowers)
                {
                    int xPos = baseX + widthUsed % collectionWidth * 76;
                    int yPos = baseY + widthUsed / collectionWidth * 76;
                    bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
                    string name = TokenParser.ParseText(power.Value.DisplayName);
                    string description = TokenParser.ParseText(power.Value.Description);
                    Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
                    string category = TokenParser.ParseText(power.Value.Category);
                    if (this.vanillaPowers.Count == 0 || yPos > base.yPositionOnScreen + base.height - 128)
                    {
                        this.vanillaPowers.Add(new List<ClickableTextureComponent>());
                        widthUsed = 0;
                        xPos = baseX;
                        yPos = baseY;
                    }

                    List<ClickableTextureComponent> list = this.vanillaPowers.Last();
                    list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null,
                        description, texture,
                        new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f,
                        unlocked)
                    {
                        myID = list.Count,
                        rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
                        leftNeighborID = ((list.Count % collectionWidth == 0) ? (-1) : (list.Count - 1)),
                        downNeighborID = ((yPos + 76 > base.yPositionOnScreen + base.height - 128)
                            ? (-7777)
                            : (list.Count + collectionWidth)),
                        upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
                        fullyImmutable = true
                    });
                    widthUsed++;
                }
            }
        }

        public void populateVanillaPowers()
        {
            this.vanillaPowers = new List<List<ClickableTextureComponent>>();
            Dictionary<string, PowersData> powersData = null;
            Dictionary<string, SPUData> SPUData = new Dictionary<string, SPUData>();
            try
            {
                powersData = DataLoader.Powers(Game1.content);
            }
            catch (Exception ex)
            {
                Loggers.Log("Failed to load powers data: " + ex.Message);
            }

            // convert all the powers in PowersData to SPUData objects
            foreach (KeyValuePair<string, PowersData> power in powersData)
            {
                SPUData spuData = new SPUData(power.Value);
                SPUData.Add(power.Key, spuData);
            }

            if (SPUData != null)
            {
                int collectionWidth = 9;
                int widthUsed = 0;
                int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth +
                            IClickableMenu.spaceToClearSideBorder;
                int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth +
                    IClickableMenu.spaceToClearTopBorder - 16;
                foreach (KeyValuePair<string, SPUData> power in SPUData)
                {
                    // if its not vanilla, add it to moddedPowers to deal with later
                    if (!vanillaPowerNames.Contains(power.Key))
                    {
                        moddedPowers.Add(power);
                        continue;
                    }
                    int xPos = baseX + widthUsed % collectionWidth * 76;
                    int yPos = baseY + widthUsed / collectionWidth * 76;
                    bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
                    string name = TokenParser.ParseText(power.Value.DisplayName);
                    string description = TokenParser.ParseText(power.Value.Description);
                    Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
                    string category = TokenParser.ParseText(power.Value.Category);
                    if (this.vanillaPowers.Count == 0 || yPos > base.yPositionOnScreen + base.height - 128)
                    {
                        this.vanillaPowers.Add(new List<ClickableTextureComponent>());
                        widthUsed = 0;
                        xPos = baseX;
                        yPos = baseY;
                    }

                    List<ClickableTextureComponent> list = this.vanillaPowers.Last();
                    list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null,
                        description, texture,
                        new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f,
                        unlocked)
                    {
                        myID = list.Count,
                        rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
                        leftNeighborID = ((list.Count % collectionWidth == 0) ? (-1) : (list.Count - 1)),
                        downNeighborID = ((yPos + 76 > base.yPositionOnScreen + base.height - 128)
                            ? (-7777)
                            : (list.Count + collectionWidth)),
                        upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
                        fullyImmutable = true
                    });
                    widthUsed++;
                }
            }
        }

        public override void populateClickableComponentList()
        {
            getPowers();
            populateVanillaPowers();
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

            foreach (ClickableTextureComponent c in this.vanillaPowers[this.currentPage])
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

            if (this.currentPage < this.vanillaPowers.Count - 1)
            {
                this.forwardButton.draw(b);
            }

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            foreach (ClickableTextureComponent item in this.vanillaPowers[this.currentPage])
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