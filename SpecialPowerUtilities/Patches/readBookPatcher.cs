using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SpecialPowerUtilities.Helpers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;

namespace SpecialPowerUtilities.Patches
{
    [HarmonyPatch]
    static class readBookPatcher
    {

        private static string GetBookMessage(ObjectData objData)
        {
            return objData?.CustomFields?.GetValueOrDefault("Spiderbuttons.SpecialPowerUtilities/Books/Message");
        }

        private static string GetRecipePrefix(ObjectData objData)
        {
            return objData?.CustomFields?.GetValueOrDefault("Spiderbuttons.SpecialPowerUtilities/Books/RecipePrefix");
        }
        
        public static void showMessage(StardewValley.Object obj)
        {
            if (obj.HasContextTag("spu_book_no_message")) return;
            ObjectData data = Game1.objectData[obj.ItemId];
            
            DelayedAction.functionAfterDelay(
                delegate
                {
                    Game1.showGlobalMessage(GetBookMessage(data) ?? Game1.content.LoadString("Strings\\1_6_Strings:LearnedANewPower"));
                }, 1000);
        }

        public static void showRecipeMessage(ObjectData data, int recipeCount)
        {
            DelayedAction.functionAfterDelay(
                delegate
                {
                    Game1.showGlobalMessage(GetBookMessage(data) ?? Game1.content.LoadString("Strings\\1_6_Strings:QoS_Cookbook", recipeCount.ToString() ?? ""));
                }, 1000);
        }
        
        [HarmonyPatch(typeof(StardewValley.Object), "readBook")]
        private static bool Prefix(StardewValley.Object __instance, GameLocation location)
        {
            if (__instance.HasContextTag($"!spu_recipe_book")) return true;
            ObjectData data = Game1.objectData[__instance.ItemId];
            var modID = GetRecipePrefix(data) ?? Utils.TryGetModFromString(__instance.ItemId)?.Manifest.UniqueID;
            if (modID == null)
            {
                Loggers.Log($"No valid recipe prefix found for recipe book: {__instance.Name}", LogLevel.Warn);
            }

            Game1.player.canMove = false;
            Game1.player.freezePause = 1030;
            Game1.player.faceDirection(2);
            Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
            {
                new FarmerSprite.AnimationFrame(57, 1000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
                {
                    frameEndBehavior = delegate
                    {
                        location.removeTemporarySpritesWithID(1987654);
                        Utility.addRainbowStarExplosion(location, Game1.player.getStandingPosition() + new Vector2(-40f, -156f), 8);
                    }
                }
            });
            Game1.MusicDuckTimer = 4000f;
            Game1.playSound("book_read");
            Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Rectangle(0, 0, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
            {
                holdLastFrame = true,
                id = 1987654
            });
            Color? c = ItemContextTagManager.GetColorFromTags(__instance);
            if (c.HasValue)
            {
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Book_Animation", new Rectangle(0, 20, 20, 20), 10f, 45, 1, Game1.player.getStandingPosition() + new Vector2(-48f, -156f), flicker: false, flipped: false, Game1.player.getDrawLayer() + 0.0012f, 0f, c.Value, 4f, 0f, 0f, 0f)
                {
                    holdLastFrame = true,
                    id = 1987654
                });
            }
            
            int recipeCount = RecipeBook.GrantRecipes(modID);
            showRecipeMessage(data, recipeCount);
            Game1.player.stats.Increment(__instance.ItemId);
            
            return false;
        }

        [HarmonyPatch(typeof(StardewValley.Object), "readBook")]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            ILGenerator il)
        {
            var codeInstructions = instructions.ToList();
            try
            {
                var BranchLabel = il.DefineLabel();

                int insertionIndex = -1;
                int direction = -1;
                for (var i = codeInstructions.Count - 1; i >= 0; i += direction)
                {
                    if (codeInstructions[i].opcode == OpCodes.Callvirt &&
                        codeInstructions[i].operand is MethodInfo mInfo1 && mInfo1.Equals(
                            AccessTools.Method(typeof(Stats), nameof(Stats.Increment),
                                new Type[] { typeof(string), typeof(uint) })))
                    {
                        insertionIndex = i + 2;
                        direction = 1;
                        continue;
                    }

                    if (direction == -1) continue;
                    if (codeInstructions[i].opcode == OpCodes.Call &&
                        codeInstructions[i].operand is MethodInfo mInfo2 &&
                        mInfo2.Equals(AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))))
                    {
                        codeInstructions[i].labels.Add(BranchLabel);
                        break;
                    }
                }

                var instructionsToAdd = new List<CodeInstruction>();

                instructionsToAdd.Add(new CodeInstruction(OpCodes.Ldarg_0));
                instructionsToAdd.Add(new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(readBookPatcher), nameof(showMessage))));
                instructionsToAdd.Add(new CodeInstruction(OpCodes.Br, BranchLabel));

                codeInstructions.InsertRange(insertionIndex, instructionsToAdd);

                return codeInstructions;
            }
            catch (Exception ex)
            {
                Loggers.Log("Error in SpecialPowerUtilities_readBook_Transpiler: \n" + ex, LogLevel.Error);
                return codeInstructions;
            }
        }
    }
}