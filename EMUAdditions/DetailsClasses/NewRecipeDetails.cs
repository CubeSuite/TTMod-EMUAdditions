using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions
{
    public class NewRecipeDetails
    {
        public string GUID;
        public CraftingMethod craftingMethod;
        public int craftTierRequired;
        public float duration;
        public List<RecipeResourceInfo> ingredients = new List<RecipeResourceInfo>();
        public List<RecipeResourceInfo> outputs = new List<RecipeResourceInfo>();
        public int sortPriority;
        public string unlockName;

        public string GetUniqueName() {
            return $"{GUID}-{outputs[0].name}";
        }

        public bool Validate() {
            if(outputs.Count == 0) {
                EMUAdditionsPlugin.LogError($"New Recipe cannot have no outputs");
                return false;
            }
            
            if(ingredients.Count == 0) {
                EMUAdditionsPlugin.LogError($"New Recipe cannot have no ingredients '{GetUniqueName()}'");
                return false;
            }
            
            if (string.IsNullOrEmpty(GUID)) {
                EMUAdditionsPlugin.LogError($"GUID cannot be null or empty for new Recipe '{GetUniqueName()}'");
                return false;
            }

            return true;
        }

        public SchematicsRecipeData ConvertToRecipe() {
            SchematicsRecipeData recipe = (SchematicsRecipeData)ScriptableObject.CreateInstance("SchematicsRecipeData");
            recipe.craftingMethod = craftingMethod;
            recipe.craftTierRequired = craftTierRequired;
            recipe.duration = duration;
            recipe.sortPriority = sortPriority;
            recipe.ingQuantities = new int[ingredients.Count];
            recipe.ingTypes = new ResourceInfo[ingredients.Count];
            recipe.outputQuantities = new int[outputs.Count];
            recipe.outputTypes = new ResourceInfo[outputs.Count];
            recipe.name = GetUniqueName();

            for (int i = 0; i < ingredients.Count; i++) {
                recipe.ingQuantities[i] = ingredients[i].quantity;
                ResourceInfo ingredient = ModUtils.GetResourceInfoByNameUnsafe(ingredients[i].name);
                recipe.ingTypes[i] = ingredient;
            }

            for (int i = 0; i < outputs.Count; i++) {
                recipe.outputQuantities[i] = outputs[i].quantity;
                ResourceInfo output = ModUtils.GetResourceInfoByNameUnsafe(outputs[i].name);
                recipe.outputTypes[i] = output;
            }

            return recipe;
        }
    }

    public struct RecipeResourceInfo
    {
        public string name;
        public int quantity;

        public RecipeResourceInfo(string _name, int _quantity) {
            name = _name;
            quantity = _quantity;
        }
    }
}
