using EquinoxsModUtils.Additions;
using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquinoxsModUtils.Additions.ContentAdders
{
    internal static class EquipmentAdder
    {
        // Objects & Variables
        internal static List<Equipment> equipmentToAdd = new List<Equipment>();
        internal static List<NewResourceDetails> details = new List<NewResourceDetails>();
        internal static Dictionary<string, int> idHistory => EMUAdditionsPlugin.idHistory;

        // Internal Functions

        internal static void AddHistoricEquipment() {
            List<Equipment> addedBefore = equipmentToAdd.Where(equipment => idHistory.ContainsKey($"Equipment-{equipment.info.displayName}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{addedBefore.Count} historic new Equipments have been registered for adding");

            for(int i = 0; i < addedBefore.Count; i++) {
                Equipment equipment = addedBefore[i];
                int index = equipmentToAdd.IndexOf(equipment);
                NewResourceDetails equipmentDetails = details[index];

                EquippableResourceInfo parent = (EquippableResourceInfo)ModUtils.GetResourceInfoByNameUnsafe(equipmentDetails.parentName);
                EquippableResourceInfo equipmentInfo = equipment.info;
                ModUtils.CloneObject(parent, ref equipmentInfo);

                equipmentInfo.rawName = equipmentDetails.name;
                equipmentInfo.uniqueId = idHistory[$"Equipment-{equipmentInfo.displayName}"];

                AddEquipmentToGame(equipment);
                EMUAdditionsPlugin.LogInfo($"Added historic new Equipment '{equipmentInfo.rawName}' to the game with id {equipmentInfo.uniqueId}");
            }
        }

        internal static void AddBrandNewEquipment() {
            List<Equipment> neverAdded = equipmentToAdd.Where(equipment => !idHistory.ContainsKey($"Equipment-{equipment.info.displayName}")).ToList();
            EMUAdditionsPlugin.LogInfo($"{neverAdded.Count} brand new Equipments have been registered for adding");

            for(int i = 0; i < neverAdded.Count; i++) {
                Equipment equipment = neverAdded[i];
                int index = equipmentToAdd.IndexOf(equipment);
                NewResourceDetails equipmentDetails = details[index];

                EquippableResourceInfo parent = (EquippableResourceInfo)ModUtils.GetResourceInfoByNameUnsafe(equipmentDetails.parentName);
                EquippableResourceInfo equipmentInfo = equipment.info;
                ModUtils.CloneObject(parent, ref equipmentInfo);

                equipmentInfo.rawName = equipmentDetails.name;
                equipmentInfo.uniqueId = GetNewEquipmentId();
                idHistory.Add($"Equipment-{equipmentInfo.rawName}", equipmentInfo.uniqueId);

                AddEquipmentToGame(equipment);
                EMUAdditionsPlugin.LogInfo($"Added brand new Equipment '{equipmentInfo.rawName}' to the game with id {equipmentInfo.uniqueId}");
            }

            EMUAdditionsPlugin.SaveIdHistory();
        }

        internal static void FetchUnlocks() {
            foreach(NewResourceDetails equipmentDetails in details) {
                if (string.IsNullOrEmpty(equipmentDetails.name)) continue;

                ResourceInfo info = ModUtils.GetResourceInfoByNameUnsafe(equipmentDetails.name);
                if (info.unlock != null) continue;

                info.unlock = ModUtils.GetUnlockByName(equipmentDetails.unlockName);
            }
        }

        // Private Functions

        private static void AddEquipmentToGame(Equipment equipment) {
            int index = equipmentToAdd.IndexOf(equipment);
            NewResourceDetails equipmentDetails = details[index];
            EquippableResourceInfo parent = (EquippableResourceInfo)ModUtils.GetResourceInfoByNameUnsafe(equipmentDetails.parentName);

            equipment.info.craftingMethod = equipmentDetails.craftingMethod;
            equipment.info.craftTierRequired = equipmentDetails.craftTierRequired;
            equipment.info.description = equipmentDetails.description;
            equipment.info.maxStackCount = equipmentDetails.maxStackCount;
            equipment.info.rawName = equipmentDetails.name;
            equipment.info.rawSprite = equipmentDetails.sprite;
            equipment.info.sortPriority = equipmentDetails.sortPriority;

            if (!string.IsNullOrEmpty(equipmentDetails.subHeaderTitle)) {
                equipment.info.headerType = ModUtils.GetSchematicsSubHeaderByTitle(equipmentDetails.headerTitle, equipmentDetails.subHeaderTitle);
            }

            if (equipment.info.sprite == null) {
                equipment.info.rawSprite = parent.sprite;
            }

            string displayNameHash = LocsUtility.GetHashString(equipment.info.displayName);
            string descriptionHash = LocsUtility.GetHashString(equipment.info.description);

            EMUAdditionsPlugin.customTranslations[displayNameHash] = equipment.info.displayName;
            EMUAdditionsPlugin.customTranslations[descriptionHash] = equipment.info.description;

            GameDefines.instance.resources.Add(equipment.info);
            ResourceNames.SafeResources.Add(equipment.info.displayName);
            
            Dictionary<ResourceInfo, Equipment> equipmentLookup = (Dictionary<ResourceInfo, Equipment>)ModUtils.GetPrivateField("equipmentLookup", Player.instance.equipment);
            equipmentLookup.Add(equipment.info, equipment);
            ModUtils.SetPrivateField("equipmentLookup", Player.instance.equipment, equipmentLookup);
            EMUAdditionsPlugin.LogInfo($"Registered {equipment.info.displayName} with Player.instance.equipment");
            ResourceAdder.addedIds.Add(equipment.info.uniqueId);
        }

        private static int GetNewEquipmentId() {
            int max = 0;
            foreach(ResourceInfo info in GameDefines.instance.resources) {
                if (info.uniqueId > max) max = info.uniqueId;
            }

            foreach(int id in idHistory.Values) {
                if (id > max) max = id;
            }

            return max + 1;
        }
    }
}
