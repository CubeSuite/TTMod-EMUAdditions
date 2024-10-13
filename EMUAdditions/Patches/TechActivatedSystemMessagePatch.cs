using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EquinoxsDebuggingTools;
using EquinoxsModUtils.Additions;

namespace EMUAdditions.Patches
{
    internal class TechActivatedSystemMessagePatch
    {
        [HarmonyPatch(typeof(TechActivatedSystemMessage), nameof(TechActivatedSystemMessage.ToString))]
        [HarmonyPrefix] 
        static void SetDefaultSprite(TechActivatedSystemMessage __instance) {
            if (!EDT.NullCheck(__instance.unlock.sprite, "Unlock Sprite")) {
                EDT.NullCheck(UIManager.instance, "UIManager");
                EDT.NullCheck(UIManager.instance.techTreeMenu, "techTreeMenu");

                TechTreeNode node = UIManager.instance.techTreeMenu.GridUI.GetNodeByUnlock(GameDefines.instance.unlocks[0]);
                EDT.NullCheck(node, "TechTreeNode");

                Sprite defaultSprite = (Sprite)ModUtils.GetPrivateField("iconSprite", node);
                EDT.NullCheck(defaultSprite, "defaultSprite");

                __instance.unlock.sprite = defaultSprite;
                GameDefines.instance.unlocks[GameDefines.instance.unlocks.Count - 1] = __instance.unlock;
                EMUAdditionsPlugin.LogInfo($"Set Unlock '{__instance.unlock.displayName}' to default sprite");
            }
        }
    }
}
