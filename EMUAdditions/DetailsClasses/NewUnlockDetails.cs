using EquinoxsModUtils.Additions.ContentAdders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions
{
    public class NewUnlockDetails
    {
        public string displayName;
        public string description;
        public Unlock.TechCategory category;
        public TechTreeState.ResearchTier requiredTier;
        public ResearchCoreDefinition.CoreType coreTypeNeeded;
        public int coreCountNeeded;
        public int treePosition;
        public List<string> dependencyNames = new List<string>();
        public Sprite sprite;

        internal bool Validate() {
            if(dependencyNames.Count > 2) {
                EMUAdditionsPlugin.LogError($"New Unlock '{displayName}' cannot have more than 2 dependencies");
                return false;
            }

            if(coreTypeNeeded != ResearchCoreDefinition.CoreType.Red && 
               coreTypeNeeded != ResearchCoreDefinition.CoreType.Green) {
                EMUAdditionsPlugin.LogError($"New Unlock '{displayName}' need to use either Red (Purple in-game) or Green (Blue in-game) cores");
                return false;
            }

            if (!EMUAdditionsPlugin.IsTranslatableStringUnique(displayName)) {
                EMUAdditionsPlugin.LogError($"New Unlock displayName '{displayName}' is not unique");
                return false;
            }

            if (!EMUAdditionsPlugin.IsTranslatableStringUnique(description)) {
                EMUAdditionsPlugin.LogWarning($"New Unlock {displayName}'s description is not unique");
            }

            return true;
        }

        internal Unlock ConvertToUnlock() {
            Unlock unlock = (Unlock)ScriptableObject.CreateInstance(typeof(Unlock));
            unlock.category = category;
            unlock.coresNeeded = new List<Unlock.RequiredCores>() {
                new Unlock.RequiredCores() {
                    type = coreTypeNeeded,
                    number = coreCountNeeded,
                }
            };
            unlock.isCoreTech = false;
            unlock.isDebugTech = false;
            unlock.numScansNeeded = 0;
            unlock.requiredTier = requiredTier;
            unlock.priority = 0;
            unlock.scanDuration = 1;
            unlock.sprite = sprite;
            unlock.treePosition = treePosition;

            string displayNameHash = LocsUtility.GetHashString(displayName);
            string descriptionHash = LocsUtility.GetHashString(description);

            unlock.displayNameHash = displayNameHash;
            unlock.descriptionHash = descriptionHash;

            EMUAdditionsPlugin.customTranslations[displayNameHash] = displayName;
            EMUAdditionsPlugin.customTranslations[descriptionHash] = description;

            UnlockAdder.unlockDependencies.Add(displayNameHash, dependencyNames);
            return unlock;
        }
    }
}
