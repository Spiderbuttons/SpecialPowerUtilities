using System.Collections.Generic;
using System.Linq;
using SpecialPowerUtilities.Helpers;
using SpecialPowerUtilities.Patches;
using StardewValley;
using StardewValley.GameData.Objects;

namespace SpecialPowerUtilities;

public static class RecipeBook
{
    public static int GrantRecipes(string prefix)
    {
        if (prefix == null) return 0;
        Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
        int num = 0;
        foreach (KeyValuePair<string, string> recipe in recipes)
        {
            if (!recipe.Key.StartsWith(prefix)) continue;
            bool didAdd = Game1.player.cookingRecipes.TryAdd(recipe.Key, 0);
            if (didAdd) num++;
        }
        return num;
    }

    public static void AddRecipeBook(string bookID)
    {
        List<string> recipeBooks = GetRecipeBooks();
        if (recipeBooks.Contains(bookID)) return;
        recipeBooks.Add(bookID);
        Game1.player.modData["Spiderbuttons.SpecialPowerUtilities/Books/RecipeBooks"] = string.Join(",", recipeBooks);
    }
    
    public static List<string> GetRecipeBooks()
    {
        if (Game1.player.modData.TryGetValue("Spiderbuttons.SpecialPowerUtilities/Books/RecipeBooks", out var modData))
        {
            return modData.Split(',').ToList();
        }
        return new List<string>();
    }

    public static void GrantRecipesAgain()
    {
        List<string> recipeBooks = GetRecipeBooks();
        if (recipeBooks.Count == 0) return;
        foreach (string book in recipeBooks)
        {
            if (!Game1.objectData.TryGetValue(book, out var data)) continue;
            if (data == null) continue;
            string prefix = readBookPatcher.GetRecipePrefix(data) ?? Utils.TryGetModFromString(book)?.Manifest.UniqueID;
            if (prefix == null) continue;
            GrantRecipes(prefix);
        }
    }
}