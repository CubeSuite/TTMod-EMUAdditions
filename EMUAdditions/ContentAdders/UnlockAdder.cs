using EquinoxsModUtils;
using FluffyUnderware.DevTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EMUAdditions.ContentAdders
{
    internal static class UnlockAdder
    {
        // Objects & Variables
        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();
        internal static Dictionary<string, List<string>> unlockDependencies = new Dictionary<string, List<string>>();
        internal static List<Unlock> unlocksToAdd = new List<Unlock>();
        
        private static List<string> newUnlockHashedNames = new List<string>();
        private static string dataFolder => EMUAdditionsPlugin.dataFolder;

        // Internal Functions

        internal static void AddRegisteredUnlocks() {
            EMUAdditionsPlugin.LogInfo($"{unlocksToAdd.Count} new Unlocks have been registered for adding");

            List<Unlock> addedBefore = unlocksToAdd.Where(unlock => idHistory.ContainsKey(unlock.displayNameHash)).ToList();
            for (int i = 0; i < addedBefore.Count; i++) {
                Unlock unlock = addedBefore[i];
                if (!ModUtils.NullCheck(unlock, $"New Unlock")) continue;
                if (!FindDependencies(ref unlock)) continue;

                unlock.uniqueId = idHistory[unlock.displayNameHash];
                AddUnlockToGame(ref unlock);

                EMUAdditionsPlugin.LogInfo($"Added historic new Unlock '{EMUAdditionsPlugin.customTranslations[unlock.displayNameHash]}' to the game with id {unlock.uniqueId}");
            }

            List<Unlock> neverAdded = unlocksToAdd.Where(unlock => !idHistory.ContainsKey(unlock.displayNameHash)).ToList();
            for(int i = 0; i < neverAdded.Count; i++) {
                Unlock unlock = neverAdded[i];
                if (!ModUtils.NullCheck(unlock, $"New Unlock")) continue;
                if (!FindDependencies(ref unlock)) continue;

                unlock.uniqueId = GetNewUnlockID();
                idHistory.Add(unlock.displayNameHash, unlock.uniqueId);
                AddUnlockToGame(ref unlock);

                EMUAdditionsPlugin.LogInfo($"Added brand new Unlock '{EMUAdditionsPlugin.customTranslations[unlock.displayNameHash]}' to the game with id {unlock.uniqueId}");
            }

            SaveIdHistory();
        }

        internal static void CleanUnlockStates() {
            EMUAdditionsPlugin.Log.LogInfo("Cleaning Unlock States");
            for (int i = 0; i < TechTreeState.instance.unlockStates.Count();) {
                TechTreeState.UnlockState state = TechTreeState.instance.unlockStates[i];
                if (state.unlockRef == null || GameDefines.instance.unlocks.Contains(state.unlockRef)) {
                    i++;
                    continue;
                }

                TechTreeState.instance.unlockStates.RemoveAt(i);
                GameState.instance.acknowledgedUnlocks.Remove(state.unlockRef.uniqueId);
                EMUAdditionsPlugin.Log.LogInfo($"Could not find Unlock for UnlockState #{i}. Removed.");
            }

            EMUAdditionsPlugin.Log.LogInfo($"Clearing duplicate unlock states");
            List<TechTreeState.UnlockState> uniqueStates = new List<TechTreeState.UnlockState>();
            for (int i = 0; i < TechTreeState.instance.unlockStates.Count(); i++) {
                bool isUnique = true;
                foreach (TechTreeState.UnlockState state in uniqueStates) {
                    if (TechTreeState.instance.unlockStates[i].unlockRef == null || state.unlockRef == null) continue;
                    if (TechTreeState.instance.unlockStates[i].unlockRef.uniqueId == state.unlockRef.uniqueId) {
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique) uniqueStates.Add(TechTreeState.instance.unlockStates[i]);
            }

            int numDuplicates = TechTreeState.instance.unlockStates.Count() - uniqueStates.Count;
            EMUAdditionsPlugin.Log.LogInfo($"Found '{uniqueStates.Count}' unique states");
            EMUAdditionsPlugin.Log.LogInfo($"Removing {numDuplicates} duplicates");
            TechTreeState.instance.unlockStates = uniqueStates.ToArray();
        }

        internal static void CleanTechProgress() {
            EMUAdditionsPlugin.Log.LogInfo("Cleaning Tech Progress");
            for (int i = 0; i < SaveState.instance.techTree.researchProgress.Count;) {
                SaveState.TechTreeSaveInfo.TechProgress progress = SaveState.instance.techTree.researchProgress[i];
                if (progress.techIndex >= TechTreeState.instance.unlockStates.Length) {
                    SaveState.instance.techTree.researchProgress.RemoveAt(i);
                    EMUAdditionsPlugin.Log.LogInfo($"Could not find UnlockState for TechProgress #{progress.techIndex}. Removed.");
                }
                else {
                    ++i;
                }
            }
        }

        // Private Functions

        private static void AddUnlockToGame(ref Unlock unlock) {
            newUnlockHashedNames.Add(unlock.displayNameHash);
            GameDefines.instance.unlocks.Add(unlock);
        }

        private static int GetNewUnlockID() {
            // ToDo: test GameDefines.instance.unlocks.Last().uniqueId + 1;
            int max = 0;
            for(int i = 0; i < GameDefines.instance.unlocks.Count; i++) {
                if (GameDefines.instance.unlocks[i].uniqueId > max) {
                    max = GameDefines.instance.unlocks[i].uniqueId;
                }
            }

            foreach(int id in idHistory.Values) {
                if (id > max) max = id;
            }

            return max + 1;
        }

        private static bool FindDependencies(ref Unlock unlock) {
            List<Unlock> dependencies = new List<Unlock>();
            List<string> dependencyNames = unlockDependencies[unlock.displayNameHash];
            foreach(string unlockName in dependencyNames) {
                Unlock dependency = ModUtils.GetUnlockByName(unlockName);
                if (dependency != null) {
                    dependencies.Add(dependency);
                }
                else {
                    EMUAdditionsPlugin.LogError($"Could not find dependency with name '{unlockName}'. Abandoning attempt to add.");
                    EMUAdditionsPlugin.LogError("Try using a name from EMU.UnlockNames");
                    return false;
                }
            }

            unlock.dependencies = dependencies;
            if(dependencies.Count >= 1) unlock.dependency1 = dependencies[0];
            if(dependencies.Count == 2) unlock.dependency2 = dependencies[1];
            return true;
        }

        // Data Functions

        internal static void SaveIdHistory() {
            Directory.CreateDirectory(dataFolder);
            List<string> filesLines = new List<string>();
            foreach(KeyValuePair<string, int> pair in idHistory) {
                filesLines.Add($"{pair.Key}|{pair.Value}");
            }

            string saveFile = $"{dataFolder}/Unlock Id History.txt";
            File.WriteAllLines(saveFile, filesLines);
        }

        internal static void LoadIdHistory() {
            string saveFile = $"{dataFolder}/Unlock Id History.txt";
            if (!File.Exists(saveFile)) {
                EMUAdditionsPlugin.LogWarning($"No Unlock Id History save file found");
                return;
            }

            string[] fileLines = File.ReadAllLines(saveFile);
            foreach(string line in fileLines) {
                string[] parts = line.Split('|');
                idHistory.Add(parts[0], int.Parse(parts[1]));
            }
        }
    }
}
