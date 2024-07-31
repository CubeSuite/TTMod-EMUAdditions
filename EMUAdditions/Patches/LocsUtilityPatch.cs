using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions.Patches
{
    internal class LocsUtilityPatch
    {
        [HarmonyPatch(typeof(LocsUtility), nameof(LocsUtility.TranslateStringFromHash), new Type[] { typeof(string), typeof(string), typeof(UnityEngine.Object) })]
        [HarmonyPrefix]
        private static bool GetModdedTranslation(ref string __result, string hash) {
            if (string.IsNullOrEmpty(hash)) return true;

            if (EMUAdditionsPlugin.customTranslations.ContainsKey(hash)) {
                __result = EMUAdditionsPlugin.customTranslations[hash];
                return false;
            }

            return true;
        }
    }
}
