using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpecialPowerUtilities.Components;
using StardewValley;
using StardewModdingAPI;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Models;
using StardewValley.Menus;
using StardewValley.GameData.Powers;
using StardewValley.TokenizableStrings;

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

        public int region_forwardButton = 707;

        public int region_backButton = 706;

        public int region_scrollUp = 700;

        public int region_scrollDown = 701;

        public int distanceFromMenuBottomBeforeNewPage = 128;

        public int widthToMoveActiveTab = 8;

        public ClickableTextureComponent backButton;

        public ClickableTextureComponent forwardButton;

        public ClickableTextureComponent scrollUp;

        public ClickableTextureComponent scrollDown;

        public Dictionary<string, RClickableTextureComponent> sideTabs =
            new Dictionary<string, RClickableTextureComponent>();

        public Dictionary<string, ClickableTextureComponent> tabIcons =
            new Dictionary<string, ClickableTextureComponent>();

        public Dictionary<string, List<List<ClickableTextureComponent>>> sections =
            new Dictionary<string, List<List<ClickableTextureComponent>>>();

        public Dictionary<string, ModSectionData> modSectionData = new Dictionary<string, ModSectionData>();

        public string currentTab = i18n.HoverText_StardewValley();

        public string currentTabIcon = i18n.HoverText_StardewValley();

        public int currentPage;

        private string descriptionText = "";

        private string hoverText = "";

        private int scrollTrack = 0;
        
        OrderedDictionary allPowers = new OrderedDictionary();
        
        List<KeyValuePair<string, object>> powerPlacements = new();

        private List<string> availablePowers;

        private List<string> unavailablePowers;

        Texture2D BlankTab = SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/BlankTab.png");
        
        Texture2D CatIcon = SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Cat.png");

        Texture2D ConcernedApeIcon =
            SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/ConcernedApe.png");

        Texture2D CrystalIcon =
            SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Crystal.png");

        Texture2D GiftIcon = SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Gift.png");

        Texture2D MysteryIcon =
            SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Mystery.png");

        Texture2D PufferIcon = SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Puffer.png");

        Texture2D StardropIcon =
            SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Stardrop.png");

        Texture2D TicketIcon = SpecialPowerUtilities.ModHelper.ModContent.Load<Texture2D>("assets/TabIcons/Ticket.png");

        List<Texture2D> BackupIcons = new List<Texture2D>();

        public SPUTab(int x, int y, int width, int height) : base(x, y, width, height)
        {
            BackupIcons.Add(CatIcon);
            BackupIcons.Add(CrystalIcon);
            BackupIcons.Add(GiftIcon);
            BackupIcons.Add(MysteryIcon);
            BackupIcons.Add(PufferIcon);
            BackupIcons.Add(StardropIcon);
            BackupIcons.Add(TicketIcon);

            this.sideTabs.Add(i18n.HoverText_StardewValley(), new RClickableTextureComponent(0.ToString() ?? "",
                new Rectangle(base.xPositionOnScreen - 48 + widthToMoveActiveTab,
                    base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "",
                i18n.HoverText_StardewValley(),
                Game1.mouseCursors,
                new Rectangle(16, 368, 16, 16), 4f)
            {
                myID = 7001 + sideTabs.Count,
                downNeighborID = -99998,
                upNeighborID = 12346,
                rightNeighborID = 0,
                leftNeighborID = region_scrollDown
            });
            this.tabIcons.Add(i18n.HoverText_StardewValley(), new ClickableTextureComponent(0.ToString() ?? "",
                new Rectangle(base.xPositionOnScreen - 32 + widthToMoveActiveTab,
                    base.yPositionOnScreen + 64 * (2 + this.tabIcons.Count) + 10, 64, 64), "",
                i18n.HoverText_StardewValley(),
                ConcernedApeIcon,
                new Rectangle(0, 0, ConcernedApeIcon.Width, ConcernedApeIcon.Height),
                64f / ConcernedApeIcon.Width / 1.5f)
            {
                myID = -7777 + tabIcons.Count,
                downNeighborID = -99998,
                rightNeighborID = 0
            });

            this.sections.Add(i18n.HoverText_StardewValley(), new List<List<ClickableTextureComponent>>());

            unavailablePowers = PowerState.getUnavailablePowers(Game1.player);

            loadPowers();
            reorderPowers();
            separateMods();

            if (sections.ContainsKey(i18n.HoverText_Misc()))
            {
                List<List<ClickableTextureComponent>> misc = sections[i18n.HoverText_Misc()];
                sections.Remove(i18n.HoverText_Misc());
                sections.Add(i18n.HoverText_Misc(), misc);
            }

            foreach (var tab in sections)
            {
                if (tab.Key == i18n.HoverText_StardewValley()) continue;
                Texture2D ModdedIcon;
                if (BackupIcons.Count > 0)
                {
                    ModdedIcon = BackupIcons[0];
                    BackupIcons.RemoveAt(0);
                }
                else
                {
                    ModdedIcon = PufferIcon;
                }

                if (tab.Key == i18n.HoverText_Misc())
                {
                    this.sideTabs.Add(tab.Key, new RClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 48,
                            base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "", tab.Key,
                        Game1.mouseCursors,
                        new Rectangle(16, 368, 16, 16), 4f)
                    {
                        myID = 7001 + sideTabs.Count,
                        upNeighborID = 7001 + sideTabs.Count - 1,
                        downNeighborID = 7001 + sideTabs.Count + 1,
                        rightNeighborID = 0,
                        leftNeighborID = region_scrollDown
                    });
                    this.tabIcons.Add(tab.Key, new ClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 32,
                            base.yPositionOnScreen + 64 * (2 + this.tabIcons.Count) + 10, 64, 64), "", tab.Key,
                        ModdedIcon,
                        new Rectangle(0, 0, ModdedIcon.Width, ModdedIcon.Height), 64f / ModdedIcon.Width / 1.5f)
                    {
                        myID = 2000 + tabIcons.Count,
                        upNeighborID = 2000,
                        downNeighborID = -99998,
                        rightNeighborID = 0,
                        leftNeighborID = region_scrollDown
                    });
                }
                else
                {
                    if (modSectionData[tab.Key].IconPath != null)
                    {
                        ModdedIcon = Game1.content.Load<Texture2D>(modSectionData[tab.Key].IconPath);
                    }

                    string hText = modSectionData[tab.Key].TabDisplayName ?? modSectionData[tab.Key].SectionName;

                    this.sideTabs.Add(tab.Key, new RClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 48,
                            base.yPositionOnScreen + 64 * (2 + this.sideTabs.Count), 64, 64), "",
                        hText,
                        Game1.mouseCursors,
                        new Rectangle(16, 368, 16, 16), 4f)
                    {
                        myID = 7001 + sideTabs.Count,
                        upNeighborID = 7001 + sideTabs.Count - 1,
                        downNeighborID = 7001 + sideTabs.Count + 1,
                        rightNeighborID = 0,
                        leftNeighborID = region_scrollDown
                    });
                    this.tabIcons.Add(tab.Key, new ClickableTextureComponent(0.ToString() ?? "",
                        new Rectangle(base.xPositionOnScreen - 32,
                            base.yPositionOnScreen + 64 * (2 + this.tabIcons.Count) + 10, 64, 64), "",
                        hText,
                        ModdedIcon,
                        new Rectangle(0, 0, ModdedIcon.Width, ModdedIcon.Height), 64f / ModdedIcon.Width / 1.5f)
                    {
                        myID = 2000 + tabIcons.Count,
                        upNeighborID = 2000,
                        downNeighborID = -99998,
                        rightNeighborID = 0
                    });
                }
            }

            this.sideTabs[i18n.HoverText_StardewValley()].upNeighborID = -1;
            this.sideTabs[i18n.HoverText_StardewValley()].upNeighborImmutable = true;

            string last_tab = i18n.HoverText_StardewValley();
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

            this.backButton = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + height - 80, 48, 44),
                Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
            {
                myID = region_backButton,
                upNeighborID = 1007,
            };
            this.forwardButton = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen + width - 32 - 60, base.yPositionOnScreen + height - 80, 48, 44),
                Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
            {
                myID = region_forwardButton,
            };
            this.scrollUp = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen - 104,
                    base.yPositionOnScreen + 64 * 2 + 10, 48, 44),
                Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
            {
                myID = region_scrollUp,
                downNeighborID = region_scrollDown,
                leftNeighborID = -7777,
                rightNeighborID = 7001,
            };
            
            this.scrollDown = new ClickableTextureComponent(
                new Rectangle(base.xPositionOnScreen - 104,
                    base.yPositionOnScreen + 64 * 9 + 12, 48, 44),
                Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
            {
                myID = region_scrollDown,
                upNeighborID = region_scrollUp,
                leftNeighborID = -7777,
                rightNeighborID = 1007 + scrollTrack,
            };
        }

        public override void populateClickableComponentList()
        {
            base.populateClickableComponentList();
            foreach (KeyValuePair<string, RClickableTextureComponent> v in sideTabs)
            {
                this.allClickableComponents.Add(v.Value);
            }

            foreach (KeyValuePair<string, List<List<ClickableTextureComponent>>> v in sections)
            {
                foreach (List<ClickableTextureComponent> list in v.Value)
                {
                    foreach (ClickableTextureComponent c in list)
                    {
                        this.allClickableComponents.Add(c);
                    }
                }
            }

            recalculateNeighbours();
        }

        public void recalculateNeighbours()
        {
            if (sideTabs.Count < 8)
            {
                foreach (KeyValuePair<string, RClickableTextureComponent> v in sideTabs)
                {
                    v.Value.leftNeighborID = -7777;
                }
            }
            else
            {
                for (int i = 0; i < sideTabs.Count; i++)
                {
                    if (i < scrollTrack + 4)
                    {
                        sideTabs.Values.ElementAt(i).leftNeighborID = scrollTrack > 0 ? region_scrollUp :
                            scrollTrack >= sideTabs.Count - 8 ? -7777 : region_scrollDown;
                    }
                    else
                    {
                        sideTabs.Values.ElementAt(i).leftNeighborID = scrollTrack < sideTabs.Count - 8
                            ? region_scrollDown
                            : scrollTrack <= 0
                                ? -7777
                                : region_scrollUp;
                    }
                }

                scrollUp.rightNeighborID = 7001;
                scrollDown.rightNeighborID = 1007 + scrollTrack;
            }

            int index = 0;
            foreach (KeyValuePair<string, List<List<ClickableTextureComponent>>>
                         category in sections)
            {
                if (category.Key == currentTab) break;
                index++;
            }
            
            if (currentPage > 0)
            {
                for (var j = sections[currentTab][currentPage].Count - 1; (j + 1) % 9 != 0; j--)
                {
                    ClickableTextureComponent c = sections[currentTab][currentPage][j];
                    c.downNeighborID = region_backButton;
                }
            }

            foreach (KeyValuePair<string, RClickableTextureComponent> v in sideTabs)
            {
                v.Value.rightNeighborID = index * 100;
            }

            scrollDown.rightNeighborID = 7001 + sideTabs.Count - 1;
            scrollDown.upNeighborID = scrollTrack <= 0 ? -7777 : region_scrollUp;
            scrollUp.downNeighborID = scrollTrack >= sideTabs.Count - 8 ? -7777 : region_scrollDown;
        }

        public override void snapToDefaultClickableComponent()
        {
            base.snapToDefaultClickableComponent();
            base.currentlySnappedComponent = base.getComponentWithID(0);
            this.snapCursorToCurrentSnappedComponent();
        }
        
        public int getPowerIndex(string power)
        {
            int index = 0;
            foreach (DictionaryEntry powerData in allPowers)
            {
                if (powerData.Key as string == power) return index;
                index++;
            }

            return -1;
        }

        public void reorderPowers()
        {
            foreach (DictionaryEntry power in allPowers)
            {
                PowersData pData = power.Value as PowersData;
                if (pData == null) continue;
                if (pData.CustomFields != null && pData.CustomFields.TryGetValue("Spiderbuttons.SpecialPowerUtilities/Placement/ToPosition", out var placementData) && placementData is string placementVal)
                {
                    if (placementVal.Equals("Top")) powerPlacements.Add(new KeyValuePair<string, object>(power.Key as string, 0));
                    else if (placementVal.Equals("Bottom")) powerPlacements.Add(new KeyValuePair<string, object>(power.Key as string, -1));
                    else if (int.TryParse(placementVal, out var placementInt))
                    {
                        powerPlacements.Add(new KeyValuePair<string, object>(power.Key as string, placementInt));
                    } else
                    {
                        Loggers.Log("Invalid placement value for power " + power.Key + ": " + placementVal, LogLevel.Warn);
                    }
                } else if (pData.CustomFields != null && pData.CustomFields.TryGetValue("Spiderbuttons.SpecialPowerUtilities/Placement/BeforeID", out var beforeData) && beforeData is string beforeVal)
                {
                    powerPlacements.Add(new KeyValuePair<string, object>(power.Key as string, new KeyValuePair<string, object>("Before", beforeVal)));
                } else if (pData.CustomFields != null && pData.CustomFields.TryGetValue("Spiderbuttons.SpecialPowerUtilities/Placement/AfterID", out var afterData) && afterData is string afterVal)
                {
                    powerPlacements.Add(new KeyValuePair<string, object>(power.Key as string, new KeyValuePair<string, object>("After", afterVal)));
                }
            }
            
            foreach (var placement in powerPlacements)
            {
                string power = placement.Key;
                if (placement.Value is int position)
                {
                    if (allPowers.Contains(power))
                    {
                        if (position > allPowers.Count)
                        {
                            position = -1;
                        }
                        var powerData = allPowers[power];
                        allPowers.Remove(power);
                        allPowers.Insert(position == -1 ? allPowers.Count : position, power, powerData);
                    }
                } else if (placement.Value is KeyValuePair<string, object> beforeOrAfter)
                {
                    string beforeOrAfterPower = beforeOrAfter.Value as string;
                    if (allPowers.Contains(power) && allPowers.Contains(beforeOrAfterPower))
                    {
                        var powerData = allPowers[power];
                        allPowers.Remove(power);
                        int index = getPowerIndex(beforeOrAfterPower);
                        if (beforeOrAfter.Key == "Before")
                        {
                            allPowers.Insert(index, power, powerData);
                        }
                        else
                        {
                            allPowers.Insert(index + 1, power, powerData);
                        }
                    }
                }
            }
        }

        public void loadPowers()
        {
            try
            {
                var powersData = DataLoader.Powers(Game1.content);
                foreach (KeyValuePair<string, PowersData> power in powersData)
                {
                    allPowers.Add(power.Key, power.Value);
                }
            }
            catch (Exception ex)
            {
                Loggers.Log("Failed to load powers data: " + ex.Message);
            }

            if (!SpecialPowerUtilities.Config.EnableCategories) return;
            var cats = Game1.content.Load<Dictionary<string, ModSectionData>>(
                "Spiderbuttons.SpecialPowerUtilities/PowerTabs");
            var cats2 = Game1.content.Load<Dictionary<string, ModSectionData>>(
                "Spiderbuttons.SpecialPowerUtilities/PowerSections");
            cats = cats.Concat(cats2).ToDictionary(x => x.Key, x => x.Value);
            foreach (var cat in cats)
            {
                if (!sections.ContainsKey(cat.Key))
                {
                    sections.Add(cat.Key, new List<List<ClickableTextureComponent>>());
                    modSectionData.Add(cat.Key, cat.Value);
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
            
            foreach (DictionaryEntry power in allPowers)
            {
                PowersData pData = power.Value as PowersData;
                if (pData == null) continue;
                string whichCategory;
                string parsedID = Utils.TryGetModFromString(power.Key as string)?.Manifest.UniqueID ??
                    i18n.HoverText_Misc();
                if (vanillaPowerNames.Contains(power.Key)) whichCategory = i18n.HoverText_StardewValley();
                else if (pData.CustomFields != null && pData.CustomFields.TryGetValue(
                             "Spiderbuttons.SpecialPowerUtilities/Tab",
                             out var section) && section is string catVal)
                {
                    whichCategory = catVal == "Stardew Valley" ? i18n.HoverText_StardewValley() : catVal;
                }
                else if (pData.CustomFields != null && pData.CustomFields.TryGetValue(
                             "Spiderbuttons.SpecialPowerUtilities/Section",
                             out var sectionOBS) && sectionOBS is string catValOBS)
                {
                    whichCategory = catValOBS == "Stardew Valley" ? i18n.HoverText_StardewValley() : catValOBS;
                }
                else if (SpecialPowerUtilities.Config.ParseModNames || sections.ContainsKey(parsedID))
                {
                    whichCategory = parsedID;
                }
                else
                {
                    whichCategory = i18n.HoverText_Misc();
                }

                if (!SpecialPowerUtilities.Config.EnableMiscCategory && whichCategory == i18n.HoverText_Misc())
                    whichCategory = i18n.HoverText_StardewValley();
                
                if (!SpecialPowerUtilities.Config.EnableCategories) whichCategory = i18n.HoverText_StardewValley();
                
                if (!sections.ContainsKey(whichCategory))
                {
                    sections.Add(whichCategory, new List<List<ClickableTextureComponent>>());
                    modSectionData.Add(whichCategory, new ModSectionData()
                    {
                        TabDisplayName = whichCategory.Contains('.') ? whichCategory.Split(".")[1] : whichCategory,
                        IconPath = null,
                    });
                }

                if (!widthUsed.ContainsKey(whichCategory)) widthUsed.Add(whichCategory, 0);
                int xPos = baseX + widthUsed[whichCategory] % categoryWidth * 76;
                int yPos = baseY + widthUsed[whichCategory] / categoryWidth * 76;
                ;
                if (yPos > base.yPositionOnScreen + height - distanceFromMenuBottomBeforeNewPage)
                {
                    sections[whichCategory].Add(new List<ClickableTextureComponent>());
                    widthUsed[whichCategory] = 0;
                    xPos = baseX;
                    yPos = baseY;
                }

                if (!sections.ContainsKey(whichCategory))
                {
                    sections.Add(whichCategory, new List<List<ClickableTextureComponent>>());
                }

                if (sections[whichCategory].Count == 0)
                {
                    sections[whichCategory].Add(new List<ClickableTextureComponent>());
                }

                int index = 0;
                foreach (KeyValuePair<string, List<List<ClickableTextureComponent>>>
                             category in sections)
                {
                    if (category.Key == whichCategory) break;
                    index++;
                }

                index *= 100;
                List<ClickableTextureComponent> list = sections[whichCategory].Last();
                bool unlocked = GameStateQuery.CheckConditions(pData.UnlockedCondition);
                string name = TokenParser.ParseText(pData.DisplayName);
                string description = TokenParser.ParseText(pData.Description);
                Texture2D texture = Game1.content.Load<Texture2D>(pData.TexturePath);
                list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null, description,
                    texture, new Rectangle(pData.TexturePosition.X, pData.TexturePosition.Y, 16, 16), 4f,
                    unlocked)
                {
                    myID = (index + list.Count + (54 * (sections[whichCategory].Count - 1))),
                    rightNeighborID = (((list.Count + 1) % categoryWidth == 0) ? (-1) : ((index + list.Count + (54 * (sections[whichCategory].Count - 1))) + 1)),
                    leftNeighborID = ((list.Count % categoryWidth == 0) ? (SpecialPowerUtilities.Config.EnableCategories ? 7001 : -1) : ((index + list.Count + (54 * (sections[whichCategory].Count - 1))) - 1)),
                    downNeighborID =
                        ((yPos + 76 > base.yPositionOnScreen + height - distanceFromMenuBottomBeforeNewPage)
                            ? (-7777)
                            : ((index + list.Count + (54 * (sections[whichCategory].Count - 1))) + categoryWidth)),
                    upNeighborID = ((list.Count < categoryWidth) ? 12346 : ((index + list.Count + (54 * (sections[whichCategory].Count - 1))) - categoryWidth)),
                    fullyImmutable = true
                });
                widthUsed[whichCategory]++;
            }
        }

        public void switchTab(string tabToSwitchTo)
        {
            if (sideTabs.ContainsKey(tabToSwitchTo))
            {
                sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
                tabIcons[currentTabIcon].bounds.X -= widthToMoveActiveTab;
                currentTab = tabToSwitchTo;
                currentTabIcon = tabToSwitchTo;
                currentPage = 0;
                sideTabs[currentTab].bounds.X += widthToMoveActiveTab;
                tabIcons[currentTabIcon].bounds.X += widthToMoveActiveTab;
            }
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
            switch (direction)
            {
                case 2:
                    if (this.currentPage > 0)
                    {
                        base.currentlySnappedComponent = base.getComponentWithID(region_backButton);
                        this.backButton.upNeighborID = (54 * currentPage) + sections[currentTab][currentPage].Count - 1;
                    }
                    else if (this.currentPage == 0 && this.sections[this.currentTab].Count > 1)
                    {
                        base.currentlySnappedComponent = base.getComponentWithID(region_forwardButton);
                        this.forwardButton.upNeighborID = sections[currentTab][currentPage].Count - 1;
                    }
                    
                    break;
                case 3:
                    if (oldID == region_forwardButton && this.currentPage > 0)
                    {
                        base.currentlySnappedComponent = base.getComponentWithID(region_backButton);
                        this.backButton.upNeighborID = (54 * currentPage) + sections[currentTab][currentPage].Count - 1;
                    }

                    break;
                case 1:
                    if (oldID == region_backButton && this.sections[this.currentTab].Count > this.currentPage + 1)
                    {
                        base.currentlySnappedComponent = base.getComponentWithID(region_forwardButton);
                        this.forwardButton.upNeighborID = sections[currentTab][currentPage].Count - 1;
                    }

                    break;
            }
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            if (this.currentlySnappedComponent.myID >= 7001 &&
                this.currentlySnappedComponent.myID < 7001 + sideTabs.Count)
            {
                if (this.currentlySnappedComponent.myID < 7001 + scrollTrack)
                {
                    while (this.currentlySnappedComponent.myID < 7001 + scrollTrack)
                    {
                        this.currentlySnappedComponent =
                            this.getComponentWithID(this.currentlySnappedComponent.myID + 1);
                        Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                    }
                }
                else if (this.currentlySnappedComponent.myID >= 7001 + scrollTrack + 8)
                {
                    while (this.currentlySnappedComponent.myID >= 7001 + scrollTrack + 8)
                    {
                        this.currentlySnappedComponent =
                            this.getComponentWithID(this.currentlySnappedComponent.myID - 1);
                        Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
                    }
                }
            }

            if (this.currentlySnappedComponent.myID == 7001 && this.sideTabs.Count() == 1)
            {
                this.currentlySnappedComponent = this.getComponentWithID(0);
                Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
            }

            recalculateNeighbours();
            base.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (scrollDown.containsPoint(x, y) && scrollTrack < sideTabs.Count - 8)
            {
                scrollTrack++;
                Game1.playSound("shwip");
                scrollDown.scale = scrollDown.baseScale;
                for (int i = 0; i < sideTabs.Count; i++)
                {
                    sideTabs.Values.ElementAt(i).bounds.Y -= 64;
                    tabIcons.Values.ElementAt(i).bounds.Y -= 64;
                }

                if (currentTab == sideTabs.Keys.ElementAt(scrollTrack - 1))
                {
                    switchTab(sideTabs.Keys.ElementAt(scrollTrack));
                }
                else if (currentTab == sideTabs.Keys.ElementAt(scrollTrack + 6))
                {
                    switchTab(sideTabs.Keys.ElementAt(scrollTrack + 7));
                }

                recalculateNeighbours();
                return;
            }

            if (scrollUp.containsPoint(x, y) && scrollTrack > 0)
            {
                scrollTrack--;
                Game1.playSound("shwip");
                scrollUp.scale = scrollUp.baseScale;
                for (int i = 0; i < sideTabs.Count; i++)
                {
                    sideTabs.Values.ElementAt(i).bounds.Y += 64;
                    tabIcons.Values.ElementAt(i).bounds.Y += 64;
                }

                if (currentTab == sideTabs.Keys.ElementAt(scrollTrack + 1))
                {
                    switchTab(sideTabs.Keys.ElementAt(scrollTrack));
                }
                else if (currentTab == sideTabs.Keys.ElementAt(scrollTrack + 8))
                {
                    switchTab(sideTabs.Keys.ElementAt(scrollTrack + 7));
                }

                recalculateNeighbours();
                return;
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
                recalculateNeighbours();
            }

            if (currentPage < sections[currentTab].Count - 1 && forwardButton.containsPoint(x, y))
            {
                currentPage++;
                Game1.playSound("shwip");
                forwardButton.scale = forwardButton.baseScale;
                if (Game1.options.snappyMenus && Game1.options.gamepadControls &&
                    currentPage == sections[currentTab].Count - 1)
                {
                    base.currentlySnappedComponent = backButton;
                    this.backButton.upNeighborID = (54 * currentPage) + sections[currentTab][currentPage].Count - 1;
                    Game1.setMousePosition(base.currentlySnappedComponent.bounds.Center);
                }
                recalculateNeighbours();
            }
            
            if (!SpecialPowerUtilities.Config.EnableCategories || this.sideTabs.Count() == 1) return;
            foreach (KeyValuePair<string, RClickableTextureComponent> v in sideTabs)
            {
                if (v.Value.containsPoint(x, y) && currentTab != v.Key)
                {
                    Game1.playSound("smallSelect");
                    sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
                    tabIcons[currentTabIcon].bounds.X -= widthToMoveActiveTab;
                    currentTab = v.Key;
                    currentTabIcon = v.Key;
                    currentPage = 0;
                    v.Value.bounds.X += widthToMoveActiveTab;
                    tabIcons[currentTabIcon].bounds.X += widthToMoveActiveTab;
                    recalculateNeighbours();
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            this.hoverText = "";
            this.descriptionText = "";
            base.performHoverAction(x, y);

            if (sections[currentTab].Count > 0)
            {
                foreach (ClickableTextureComponent c in sections[currentTab][currentPage])
                {
                    if (c.containsPoint(x, y))
                    {
                        c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
                        this.hoverText = (c.drawShadow ? c.name : "???");
                        string key = null;
                        foreach (DictionaryEntry power in allPowers)
                        {
                            PowersData pData = power.Value as PowersData;
                            if (TokenParser.ParseText(pData.DisplayName) == c.name)
                            {
                                key = power.Key as string;
                                break;
                            }
                        }
                        this.descriptionText = Game1.parseText(c.hoverText, Game1.smallFont,
                            Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverText).X, 320));
                        if (unavailablePowers.Contains(key))
                        {
                            this.descriptionText += Game1.parseText(($"\r\n\r\n{i18n.HoverText_Unavailable()}.").ToUpper(),
                                Game1.smallFont,
                                Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverText).X, 320));
                        }
                    }
                    else
                    {
                        c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
                    }
                }
            }

            this.forwardButton.tryHover(x, y, 0.5f);
            this.backButton.tryHover(x, y, 0.5f);
            this.scrollUp.tryHover(x, y, 0.5f);
            this.scrollDown.tryHover(x, y, 0.5f);
            
            if (!SpecialPowerUtilities.Config.EnableCategories || this.sideTabs.Count() == 1) return;
            foreach (ClickableTextureComponent tab in this.sideTabs.Values)
            {
                if (tab.containsPoint(x, y) && tab.bounds.Y >= base.yPositionOnScreen + 64 * 2 &&
                    tab.bounds.Y < base.yPositionOnScreen + 64 * 10)
                {
                    this.hoverText = tab.hoverText ?? "String not found!";
                    return;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            int catCount = 0;
            for (int i = 0; i < this.sideTabs.Count; i++)
            {
                if (!SpecialPowerUtilities.Config.EnableCategories || this.sideTabs.Count() == 1) break;
                if (i < scrollTrack) continue;
                if (catCount < 8)
                {
                    this.sideTabs.Values.ElementAt(i).draw(b, -(float)Math.PI / 2.0f);
                    this.tabIcons.Values.ElementAt(i).draw(b);
                }

                catCount++;
            }

            if (currentPage > 0)
            {
                this.backButton.draw(b);
            }

            if (currentPage < sections[currentTab].Count - 1)
            {
                this.forwardButton.draw(b);
            }
            
            if (scrollTrack > 0) this.scrollUp.draw(b);
            if (scrollTrack < sideTabs.Count - 8) this.scrollDown.draw(b);
            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            if (sections[currentTab].Count > 0)
            {
                foreach (ClickableTextureComponent item in sections[currentTab][currentPage])
                {
                    bool drawColor = item.drawShadow;
                    string key = null;
                    foreach (DictionaryEntry power in allPowers)
                    {
                        PowersData pData = power.Value as PowersData;
                        if (TokenParser.ParseText(pData.DisplayName) == item.name)
                        {
                            key = power.Key as string;
                            break;
                        }
                    }
                    if (!drawColor)
                    {
                        item.draw(b, Color.Black * 0.2f, 0.86f);
                    }
                    else if (unavailablePowers.Contains(key))
                    {
                        item.draw(b, Color.DimGray * 0.4f, 0.86f);
                    }
                    else
                    {
                        item.draw(b, Color.White, 0.86f);
                    }
                }
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