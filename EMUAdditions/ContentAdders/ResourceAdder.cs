using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EMUAdditions.ContentAdders
{
    internal static class ResourceAdder
    {
        // Objects & Variables
        internal static List<NewResourceDetails> resourcesToAdd = new List<NewResourceDetails>();
        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();
        internal static List<int> addedIds = new List<int>();
        
        private static string dataFolder => EMUAdditionsPlugin.dataFolder;

        // Internal Functions

        internal static void AddHistoricResources() {
            List<NewResourceDetails> addedBefore = resourcesToAdd.Where(details => idHistory.ContainsKey(details.name)).ToList();
            EMUAdditionsPlugin.LogInfo($"{addedBefore.Count} historic new Resources have been registered for adding");

            for (int i = 0; i < addedBefore.Count; i++) {
                NewResourceDetails details = addedBefore[i];

                ResourceInfo resource = details.ConvertToResourceInfo();
                resource.uniqueId = idHistory[resource.displayName];

                AddResourceToGame(details, ref resource);

                EMUAdditionsPlugin.LogInfo($"Added historic new Resource '{details.name}' to the game with id {resource.uniqueId}");
            }
        }

        internal static void AddBrandNewResources() {
            List<NewResourceDetails> neverAdded = resourcesToAdd.Where(details => !idHistory.ContainsKey(details.name)).ToList();
            EMUAdditionsPlugin.LogInfo($"{neverAdded.Count} brand new Resources have been registered for adding");
            
            for (int i = 0; i < neverAdded.Count; i++) {
                NewResourceDetails details = neverAdded[i];

                ResourceInfo resource = details.ConvertToResourceInfo();
                resource.uniqueId = GetNewResourceID();
                idHistory.Add(resource.displayName, resource.uniqueId);

                AddResourceToGame(details, ref resource);
                EMUAdditionsPlugin.LogInfo($"Added brand new Resource '{details.name}' to the game with id {resource.uniqueId}");
            }

            SaveIdHistory();
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

            resource.unlock = ModUtils.GetUnlockByNameUnsafe(details.unlockName);
            if(resource.unlock == null) {
                resource.unlock = parent.unlock;
            }

            if(resource.sprite == null) {
                resource.rawSprite = parent.sprite;
            }

            resource.model3D = parent.model3D;
            resource.rawConveyorResourcePrefab = parent.rawConveyorResourcePrefab;

            string nameHash = LocsUtility.GetHashString(details.name);
            string descriptionHash = LocsUtility.GetHashString(details.description);

            EMUAdditionsPlugin.customTranslations.Add(nameHash, details.name);
            EMUAdditionsPlugin.customTranslations.Add(descriptionHash, details.description);

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

        // Data Functions

        private static void SaveIdHistory() {
            Directory.CreateDirectory(dataFolder);
            List<string> filesLines = new List<string>();
            foreach (KeyValuePair<string, int> pair in idHistory) {
                filesLines.Add($"{pair.Key}|{pair.Value}");
            }

            string saveFile = $"{dataFolder}/Resource Id History.txt";
            File.WriteAllLines(saveFile, filesLines);
        }

        internal static void LoadIdHistory() {
            string saveFile = $"{dataFolder}/Resource Id History.txt";
            if (!File.Exists(saveFile)) {
                EMUAdditionsPlugin.LogWarning($"No Resource Id History save file found");
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
