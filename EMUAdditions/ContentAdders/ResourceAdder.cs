using EquinoxsModUtils.Additions.Patches;
using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection.Emit;

namespace EquinoxsModUtils.Additions.ContentAdders
{
    internal static class ResourceAdder
    {
        // Objects & Variables
        internal static List<NewResourceDetails> resourcesToAdd = new List<NewResourceDetails>();
        internal static Dictionary<string, int> idHistory => EMUAdditionsPlugin.idHistory;
        internal static List<int> addedIds = new List<int>();

        // Internal Functions

        internal static void AddHistoricResources() {
            List<NewResourceDetails> addedBefore = resourcesToAdd.Where(details => idHistory.ContainsKey($"Resource-{details.name}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{addedBefore.Count} historic new Resources have been registered for adding");

            for (int i = 0; i < addedBefore.Count; i++) {
                NewResourceDetails details = addedBefore[i];

                ResourceInfo resource = details.ConvertToResourceInfo();
                resource.uniqueId = idHistory[$"Resource-{details.name}"];

                AddResourceToGame(details, ref resource);

                EMUAdditionsPlugin.LogInfo($"Added historic new Resource '{details.name}' to the game with id {resource.uniqueId}");
            }
        }

        internal static void AddBrandNewResources() {
            List<NewResourceDetails> neverAdded = resourcesToAdd.Where(details => !idHistory.ContainsKey($"Resource-{details.name}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{neverAdded.Count} brand new Resources have been registered for adding");
            
            for (int i = 0; i < neverAdded.Count; i++) {
                NewResourceDetails details = neverAdded[i];

                ResourceInfo resource = details.ConvertToResourceInfo();
                resource.uniqueId = GetNewResourceID();
                idHistory.Add($"Resource-{details.name}", resource.uniqueId);

                AddResourceToGame(details, ref resource);
                EMUAdditionsPlugin.LogInfo($"Added brand new Resource '{details.name}' to the game with id {resource.uniqueId}");
            }

            EMUAdditionsPlugin.SaveIdHistory();
        }

        internal static void FillMissingIds() {
            foreach(int id in idHistory.Values) {
                if (addedIds.Contains(id)) continue;

                EMUAdditionsPlugin.LogWarning($"Historic ResourceId {id} has not added been added this launch");
                ResourceInfo parent = ModUtils.GetResourceInfoByNameUnsafe("SharkRepellant");

                ResourceInfo hiddenResoure = (ResourceInfo)ScriptableObject.CreateInstance(typeof(ResourceInfo));
                hiddenResoure.name = "EMU Hidden Resource";
                hiddenResoure.redacted = true;
                hiddenResoure.uniqueId = id;
                hiddenResoure.rawConveyorResourcePrefab = parent.rawConveyorResourcePrefab;
                hiddenResoure.headerType = parent.headerType;

                GameDefines.instance.resources.Add(hiddenResoure);
            }
        }

        internal static void FetchUnlocks() {
            foreach(NewResourceDetails details in resourcesToAdd) {
                if (string.IsNullOrEmpty(details.unlockName)) continue;
                
                ResourceInfo resource = ModUtils.GetResourceInfoByName(details.name);
                if (resource.unlock != null) continue;

                resource.unlock = ModUtils.GetUnlockByName(details.unlockName);
            }
        }

        // Private Functions

        private static void AddResourceToGame(NewResourceDetails details, ref ResourceInfo resource) {
            ResourceInfo parent = ModUtils.GetResourceInfoByNameUnsafe(details.parentName);
            if (parent == null) {
                EMUAdditionsPlugin.LogError($"Could not find parent Resource '{details.parentName}'");
                EMUAdditionsPlugin.LogError($"Abandoning attempt to add new Resource '{details.name}'");
                return;
            }

            if (!string.IsNullOrEmpty(details.subHeaderTitle)) {
                resource.headerType = ModUtils.GetSchematicsSubHeaderByTitle(details.headerTitle, details.subHeaderTitle);
            }
            
            if(resource.headerType == null) {
                resource.headerType = parent.headerType;
            }

            if(resource.sprite == null) {
                resource.rawSprite = parent.sprite;
            }

            resource.model3D = parent.model3D;
            resource.rawConveyorResourcePrefab = parent.rawConveyorResourcePrefab;

            string nameHash = LocsUtility.GetHashString(details.name);
            string descriptionHash = LocsUtility.GetHashString(details.description);

            EMUAdditionsPlugin.customTranslations[nameHash] = details.name;
            EMUAdditionsPlugin.customTranslations[descriptionHash] = details.description;

            GameDefines.instance.resources.Add(resource);
            ResourceNames.SafeResources.Add(resource.displayName);
            addedIds.Add(resource.uniqueId);
        }

        private static int GetNewResourceID() {
            int max = 0;
            foreach(ResourceInfo info in GameDefines.instance.resources) {
                if (info.uniqueId > max) max = info.uniqueId;
            }

            foreach(int id in idHistory.Values) {
                if(id > max) max = id;
            }

            return max + 1;
        }
    }
}
