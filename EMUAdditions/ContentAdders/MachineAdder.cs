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
        internal static Dictionary<string, int> idHistory => EMUAdditionsPlugin.idHistory;

        // Internal Functions

        internal static void AddHistoricMachines() {
            List<ResourceInfo> addedBefore = machinesToAdd.Where(machine => idHistory.ContainsKey($"Machine-{machine.displayName}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{addedBefore.Count} historic new Machines have been registered for adding");

            for(int i =  0; i < addedBefore.Count; i++) {
                ResourceInfo machine = addedBefore[i];
                int index = machinesToAdd.IndexOf(machine);
                NewResourceDetails machineDetails = details[index];

                BuilderInfo parent = (BuilderInfo)ModUtils.GetResourceInfoByNameUnsafe(machineDetails.parentName);
                ModUtils.CloneObject(parent, ref machine);
                
                machine.rawName = machineDetails.name;
                machine.uniqueId = idHistory[$"Machine-{machine.displayName}"];

                AddMachineToGame(machine);
                EMUAdditionsPlugin.LogInfo($"Added historic new Machine '{machine.rawName}' to the game with id {machine.uniqueId}");
            }
        }

        internal static void AddBrandNewMachines() {
            List<ResourceInfo> neverAdded = machinesToAdd.Where(machine => !idHistory.ContainsKey($"Machine-{machine.displayName}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{neverAdded.Count} brand new Machines have been registered for adding");

            for(int i = 0; i < neverAdded.Count; i++) {
                ResourceInfo machine = neverAdded[i];
                int index = machinesToAdd.IndexOf(machine);
                NewResourceDetails machineDetails = details[index];

                BuilderInfo parent = (BuilderInfo)ModUtils.GetResourceInfoByNameUnsafe(machineDetails.parentName);
                ModUtils.CloneObject(parent, ref machine);
                
                machine.rawName = machineDetails.name;
                machine.uniqueId = GetNewMachineID();
                idHistory.Add($"Machine-{machine.displayName}", machine.uniqueId);

                AddMachineToGame(machine);
                EMUAdditionsPlugin.LogInfo($"Added brand new Machine '{machineDetails.name}' to the game with id '{machine.uniqueId}'");
            }

            EMUAdditionsPlugin.SaveIdHistory();
        }

        internal static void FetchUnlocks() {
            foreach(NewResourceDetails machineDetails in details) {
                if (string.IsNullOrEmpty(machineDetails.name)) continue;

                ResourceInfo machine = ModUtils.GetResourceInfoByName(machineDetails.name);
                if (machine.unlock != null) continue;

                machine.unlock = ModUtils.GetUnlockByName(machineDetails.unlockName);
            }
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

            if (machineDefinition.sprite == null) {
                machineDefinition.rawSprite = parent.sprite;
            }

            string displayNameHash = LocsUtility.GetHashString(machineDefinition.displayName);
            string descriptionHash = LocsUtility.GetHashString(machineDefinition.description);

            EMUAdditionsPlugin.customTranslations[displayNameHash] = machineDefinition.displayName;
            EMUAdditionsPlugin.customTranslations[descriptionHash] = machineDefinition.description;

            GameDefines.instance.resources.Add(machineDefinition);
            GameDefines.instance.buildableResources.Add((BuilderInfo)machineDefinition);
            ResourceNames.SafeResources.Add(machineDefinition.displayName);
            ResourceAdder.addedIds.Add(machineDefinition.uniqueId);
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
    }
}
