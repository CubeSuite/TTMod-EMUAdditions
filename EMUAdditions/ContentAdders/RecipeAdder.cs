using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMUAdditions.ContentAdders
{
    internal static class RecipeAdder
    {
        // Objects & Variables
        internal static List<NewRecipeDetails> recipesToAdd = new List<NewRecipeDetails>();
        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();
        private static string dataFolder => EMUAdditionsPlugin.dataFolder;

        // Internal Functions

        internal static void AddRegisteredRecipes() {
            EMUAdditionsPlugin.LogInfo($"{recipesToAdd.Count} new Recipes have been registered for adding");

            List<NewRecipeDetails> addedBefore = recipesToAdd.Where(recipe => idHistory.ContainsKey(recipe.GetUniqueName())).ToList();
            foreach(NewRecipeDetails details in addedBefore) {
                SchematicsRecipeData recipe = details.ConvertToRecipe();
                recipe.uniqueId = idHistory[details.GetUniqueName()];
                AddRecipeToGame(details, ref recipe);

                EMUAdditionsPlugin.LogInfo($"Added historic new Recipe '{details.GetUniqueName()}' to the game with id {recipe.uniqueId}");
            }
            
            List<NewRecipeDetails> neverAdded = recipesToAdd.Where(recipe => !idHistory.ContainsKey(recipe.GetUniqueName())).ToList();
            foreach( NewRecipeDetails details in neverAdded) {
                SchematicsRecipeData recipe = details.ConvertToRecipe();
                recipe.uniqueId = GetNewRecipeID();
                idHistory.Add(details.GetUniqueName(), recipe.uniqueId);
                AddRecipeToGame(details, ref recipe);

                EMUAdditionsPlugin.LogInfo($"Added brand new Recipe '{details.GetUniqueName()}' to the game with id {recipe.uniqueId}");
            }

            SaveIdHistory();
        }

        // Private Functions

        private static void AddRecipeToGame(NewRecipeDetails details, ref SchematicsRecipeData recipe) {
            if (!string.IsNullOrEmpty(details.unlockName)) {
                recipe.unlock = ModUtils.GetUnlockByNameUnsafe(details.unlockName);
            }

            if (recipe.unlock == null) {
                EMUAdditionsPlugin.LogWarning($"NewRecipeDetails '{details.GetUniqueName()}' unlock is null, using outputs[0]'s instead.");
                recipe.unlock = recipe.outputTypes[0].unlock;
            }

            GameDefines.instance.schematicsRecipeEntries.Add(recipe);
        }

        private static int GetNewRecipeID() {
            int max = 0;
            
            foreach(SchematicsRecipeData recipe in GameDefines.instance.schematicsRecipeEntries) {
                if (recipe.uniqueId > max) max = recipe.uniqueId;
            }

            foreach(int id in idHistory.Values) {
                if (id > max) max = id;
            }

            return max + 1;
        }

        // Data Functions

        private static void SaveIdHistory() {
            Directory.CreateDirectory(dataFolder);
            List<string> filesLines = new List<string>();
            foreach (KeyValuePair<string, int> pair in idHistory) {
                filesLines.Add($"{pair.Key}|{pair.Value}");
            }

            string saveFile = $"{dataFolder}/Recipe Id History.txt";
            File.WriteAllLines(saveFile, filesLines);
        }

        internal static void LoadIdHistory() {
            string saveFile = $"{dataFolder}/Recipe Id History.txt";
            if (!File.Exists(saveFile)) {
                EMUAdditionsPlugin.LogWarning($"No Recipe Id History save file found");
                return;
            }

            string[] fileLines = File.ReadAllLines(saveFile);
            foreach (string line in fileLines) {
                string[] parts = line.Split('|');
                idHistory.Add(parts[0], int.Parse(parts[1]));
            }
        }
    }
}
