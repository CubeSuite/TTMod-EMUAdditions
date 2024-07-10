using EMUAdditions.ContentAdders;
using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMUAdditions.Patches
{
    internal class GameDefinesPatch
    {
        private static bool loadedCustomData = false;

        [HarmonyPatch(typeof(GameDefines), "GetMaxResId")]
        [HarmonyPrefix]
        static void AddCustomData() {
            if (loadedCustomData) return;
            loadedCustomData = true;

            SubHeaderAdder.AddRegisteredSubHeaders();
            ResourceAdder.AddHistoricResources();
            MachineAdder.AddHistoricMachines();
            ResourceAdder.AddBrandNewResources();
            MachineAdder.AddBrandNewMachines();
            ResourceAdder.FillMissingIds();
            RecipeAdder.AddRegisteredRecipes();

            ModUtils.SetPrivateStaticField("_topResId", GameDefines.instance, -1);
        }
    }
}
