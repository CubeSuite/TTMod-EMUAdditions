using EquinoxsModUtils.Additions.ContentAdders;
using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions
{
    public class NewResourceDetails
    {
        public string parentName;

        public string name;
        public string description;
        public CraftingMethod craftingMethod;
        public int craftTierRequired;
        public float fuelAmount;
        public string headerTitle;
        public string subHeaderTitle;
        public int maxStackCount = 500;
        // public GameObject model3D;
        // public GameObject rawConveyorResourcePrefab;
        public Sprite sprite;
        public int sortPriority;
        public string unlockName;

        internal bool Validate() {
            if (string.IsNullOrEmpty(parentName)) {
                EMUAdditionsPlugin.LogError($"parentName cannot be null or empty for NewResourceDetails '{name}'");
                return false;
            }

            if (string.IsNullOrEmpty(name)) {
                EMUAdditionsPlugin.LogError($"name cannot be null or empty for NewResourceDetails");
                return false;
            }

            if (string.IsNullOrEmpty(description)) {
                EMUAdditionsPlugin.LogError($"description cannot be null or empty for NewResourceDetails '{name}'");
                return false;
            }

            return true;
        }

        internal ResourceInfo ConvertToResourceInfo() {
            ResourceInfo newResource = (ResourceInfo)ScriptableObject.CreateInstance("ResourceInfo");
            newResource.craftingMethod = craftingMethod;
            newResource.craftTierRequired = craftTierRequired;
            newResource.description = description;
            newResource.fuelAmount = fuelAmount;
            newResource.maxStackCount = maxStackCount;
            // newResource.model3D = model3D;
            // newResource.rawConveyorResourcePrefab = rawConveyorResourcePrefab;
            newResource.rawName = name;
            newResource.rawSprite = sprite;
            newResource.sortPriority = sortPriority;

            return newResource;
        }
    }

    // ToDo: Uncomment GameObjects if pursuing fix to conveyor prefab issues.
}
