using EquinoxsModUtils.Additions.ContentAdders;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Additions.Patches
{
    internal class FlowManagerPatch
    {
        [HarmonyPatch(typeof(FlowManager), "ClearGameState")]
        [HarmonyPostfix]
        static void ResetContentAdders() {
            GameDefinesPatch.loadedCustomData = false;
        }
    }
}
