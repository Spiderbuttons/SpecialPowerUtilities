using System.Collections.Generic;
using System.Linq;
using SpecialPowerUtilities.Helpers;
using StardewValley;

namespace SpecialPowerUtilities;

public static class RecipeBook
{
    public static int GrantRecipes(string modID)
    {
        if (modID == null) return 0;
        Dictionary<string, string> recipes = CraftingRecipe.cookingRecipes;
        int num = 0;
        foreach (KeyValuePair<string, string> recipe in recipes)
        {
            if (!recipe.Key.StartsWith(modID)) continue;
            bool didAdd = Game1.player.cookingRecipes.TryAdd(recipe.Key, 0);
            if (didAdd) num++;
        }
        if (num > 0)
        {
            AddRecipeBook(modID);
        }
        return num;
    }

    public static void AddRecipeBook(string modID)
    {
        List<string> recipeBooks = GetRecipeBooks();
        if (recipeBooks.Contains(modID)) return;
        recipeBooks.Add(modID);
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
            GrantRecipes(book);
        }
    }
}