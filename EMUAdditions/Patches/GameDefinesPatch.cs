using EquinoxsModUtils.Additions.ContentAdders;
using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Additions.Patches
{
    internal class GameDefinesPatch
    {
        public static bool loadedCustomData = false;
        public static bool isFirstLoad = true;

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
            EquipmentAdder.AddHistoricEquipment();
            EquipmentAdder.AddBrandNewEquipment();
            ResourceAdder.FillMissingIds();
            RecipeAdder.AddRegisteredRecipes();

            ModUtils.SetPrivateStaticField("_topResId", GameDefines.instance, -1);

            isFirstLoad = false;
        }
    }
}
