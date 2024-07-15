using EquinoxsModUtils.Additions.Patches;
using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions.Must;
using static RootMotion.Demos.Turret;

namespace EquinoxsModUtils.Additions.ContentAdders
{
    internal static class MachineAdder
    {
        // Objects & Variables
        internal static List<ResourceInfo> machinesToAdd = new List<ResourceInfo>();
        internal static List<NewResourceDetails> details = new List<NewResourceDetails>();

        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();
        private static string dataFolder => EMUAdditionsPlugin.dataFolder;

        // Internal Functions

        internal static void AddHistoricMachines() {
            List<ResourceInfo> addedBefore = machinesToAdd.Where(machine => idHistory.ContainsKey(machine.displayName)).ToList();
            EMUAdditionsPlugin.LogInfo($"{addedBefore.Count} historic new Machines have been registered for adding");

            for(int i =  0; i < addedBefore.Count; i++) {
                ResourceInfo machine = addedBefore[i];
                int index = machinesToAdd.IndexOf(machine);
                NewResourceDetails machineDetails = details[index];

                BuilderInfo parent = (BuilderInfo)ModUtils.GetResourceInfoByNameUnsafe(machineDetails.parentName);
                ModUtils.CloneObject(parent, ref machine);
                
                machine.rawName = machineDetails.name;
                machine.uniqueId = idHistory[machine.displayName];

                AddMachineToGame(machine);
                EMUAdditionsPlugin.LogInfo($"Added historic new Machine '{machine.rawName}' to the game with id {machine.uniqueId}");
            }
        }

        internal static void AddBrandNewMachines() {
            List<ResourceInfo> neverAdded = machinesToAdd.Where(machine => !idHistory.ContainsKey(machine.displayName)).ToList();
            EMUAdditionsPlugin.LogInfo($"{neverAdded.Count} brand new Machines have been registered for adding");

            for(int i = 0; i < neverAdded.Count; i++) {
                ResourceInfo machine = neverAdded[i];
                int index = machinesToAdd.IndexOf(machine);
                NewResourceDetails machineDetails = details[index];

                BuilderInfo parent = (BuilderInfo)ModUtils.GetResourceInfoByNameUnsafe(machineDetails.parentName);
                ModUtils.CloneObject(parent, ref machine);
                
                machine.rawName = machineDetails.name;
                machine.uniqueId = GetNewMachineID();
                idHistory.Add(machine.displayName, machine.uniqueId);

                AddMachineToGame(machine);
                EMUAdditionsPlugin.LogInfo($"Added brand new Machine '{machine.name}' to the game with id '{machine.uniqueId}'");
            }

            SaveIdHistory();
        }

        // Private Functions

        private static void AddMachineToGame(ResourceInfo machineDefinition) {
            int index = machinesToAdd.IndexOf(machineDefinition);
            NewResourceDetails machineDetails = details[index];
            ResourceInfo parent = ModUtils.GetResourceInfoByNameUnsafe(machineDetails.parentName);
            
            machineDefinition.craftingMethod = machineDetails.craftingMethod;
            machineDefinition.craftTierRequired = machineDetails.craftTierRequired;
            machineDefinition.description = machineDetails.description;
            machineDefinition.maxStackCount = machineDetails.maxStackCount;
            machineDefinition.rawName = machineDetails.name;
            machineDefinition.rawSprite = machineDetails.sprite;
            machineDefinition.sortPriority = machineDetails.sortPriority;

            if (!string.IsNullOrEmpty(machineDetails.subHeaderTitle)) {
                machineDefinition.headerType = ModUtils.GetSchematicsSubHeaderByTitle(machineDetails.headerTitle, machineDetails.subHeaderTitle);
            }

            machineDefinition.unlock = ModUtils.GetUnlockByNameUnsafe(machineDetails.unlockName);
            if (machineDefinition.unlock == null) {
                machineDefinition.unlock = parent.unlock;
            }

            if (machineDefinition.sprite == null) {
                machineDefinition.rawSprite = parent.sprite;
            }

            if (GameDefinesPatch.isFirstLoad) {
                string displayNameHash = LocsUtility.GetHashString(machineDefinition.displayName);
                string descriptionHash = LocsUtility.GetHashString(machineDefinition.description);
                EMUAdditionsPlugin.customTranslations.Add(displayNameHash, machineDefinition.rawName);
                EMUAdditionsPlugin.customTranslations.Add(descriptionHash, machineDefinition.description);
            }

            GameDefines.instance.resources.Add(machineDefinition);
            GameDefines.instance.buildableResources.Add((BuilderInfo)machineDefinition);
            ResourceNames.SafeResources.Add(machineDefinition.displayName);
        }

        private static int GetNewMachineID() {
            int max = 0;
            foreach (ResourceInfo info in GameDefines.instance.resources) {
                if (info.uniqueId > max) max = info.uniqueId;
            }

            foreach (int id in idHistory.Values) {
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

            string saveFile = $"{dataFolder}/Machine Id History.txt";
            File.WriteAllLines(saveFile, filesLines);
        }

        internal static void LoadIdHistory() {
            string saveFile = $"{dataFolder}/Machine Id History.txt";
            if (!File.Exists(saveFile)) {
                EMUAdditionsPlugin.LogWarning($"No Machine Id History save file found");
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
