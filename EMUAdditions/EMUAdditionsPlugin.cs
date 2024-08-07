﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EquinoxsModUtils.Additions.ContentAdders;
using EquinoxsModUtils.Additions.Patches;
using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace EquinoxsModUtils.Additions
{
    internal static class Testing 
    {
        internal static bool doUnlockTest = false;
        internal static bool doResourcePlusTest = false;
        internal static bool doMachineTest = false;

        public static void DoTests() {
            if (doUnlockTest) {
                EMUAdditions.AddNewUnlock(new NewUnlockDetails() {
                    displayName = "Test Unlock 3",
                    description = "Third new test unlock",
                    category = Unlock.TechCategory.Terraforming,
                    requiredTier = TechTreeState.ResearchTier.Tier2,
                    coreTypeNeeded = ResearchCoreDefinition.CoreType.Red,
                    coreCountNeeded = 3,
                    treePosition = 10,
                });
            }

            if (doResourcePlusTest) {
                EMUAdditions.AddNewResource(new NewResourceDetails() {
                    parentName = ResourceNames.Limestone,

                    name = "Limestone 3",
                    description = "Limestone 3 Test",
                    headerTitle = "Intermediates",
                    subHeaderTitle = "Test Parts 2",
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    fuelAmount = 100f,
                    sortPriority = 10,
                    unlockName = UnlockNames.BasicManufacturing,
                });
                EMUAdditions.AddNewRecipe(new NewRecipeDetails() {
                    GUID = EMUAdditionsPlugin.MyGUID,
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    duration = 0.1f,
                    ingredients = new List<RecipeResourceInfo>() {
                        new RecipeResourceInfo() {
                            name = ResourceNames.Limestone,
                            quantity = 2
                        }
                    },
                    outputs = new List<RecipeResourceInfo>() {
                        new RecipeResourceInfo() {
                            name = "Limestone 3",
                            quantity = 2
                        }
                    },
                    sortPriority = 10,
                    unlockName = UnlockNames.BasicManufacturing
                });
                EMUAdditions.AddNewSchematicsSubHeader("Test Parts 2", "Intermediates", 10);
            }

            if (doMachineTest) {
                NewResourceDetails details = new NewResourceDetails() {
                    name = "Void Chest",
                    description = "Voids all items inserted into it.",
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    headerTitle = "Logistics",
                    subHeaderTitle = "Utility",
                    maxStackCount = 500,
                    sortPriority = 999,
                    unlockName = UnlockNames.BasicLogistics,
                    parentName = ResourceNames.Container
                };

                ChestDefinition definition;
                definition = ScriptableObject.CreateInstance<ChestDefinition>();
                EMUAdditions.AddNewMachine(definition, details, true);

                EMUAdditions.AddNewRecipe(new NewRecipeDetails() {
                    GUID = EMUAdditionsPlugin.MyGUID,
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    duration = 0.1f,
                    ingredients = new List<RecipeResourceInfo>() {
                        new RecipeResourceInfo() {
                            name = ResourceNames.IronIngot,
                            quantity = 10
                        }
                    },
                    outputs = new List<RecipeResourceInfo>() {
                        new RecipeResourceInfo() {
                            name = "Void Chest",
                            quantity = 1
                        }
                    },
                    sortPriority = 10,
                    unlockName = UnlockNames.BasicLogistics
                });
            }
        }
    }

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class EMUAdditionsPlugin : BaseUnityPlugin
    {
        // Plugin Settings
        internal const string MyGUID = "com.equinox.EMUAdditions";
        private const string PluginName = "EMUAdditions";
        private const string VersionString = "1.1.1";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        internal static ManualLogSource Log = new ManualLogSource(PluginName);

        // Objects & Variables
        internal static Dictionary<string, string> customTranslations = new Dictionary<string, string>();
        internal static string dataFolder = $"{Application.persistentDataPath}/EMUAdditions";
        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();

        // Unity Functions

        private void Awake() {
            Log = Logger;
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            LoadIdHistory();
            UnlockAdder.LoadIdHistory();
            RecipeAdder.LoadIdHistory();
            SubHeaderAdder.LoadIdHistory();

            ApplyPatches();

            ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
            ModUtils.SaveStateLoaded += OnSaveStateLoaded;
            ModUtils.GameSaved += OnGameSaved;

            Testing.DoTests();

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
        }

        private void Update() {
            // ToDo: Delete If Not Needed
        }

        // Events

        private void OnGameDefinesLoaded(object sender, EventArgs e) {
            UnlockAdder.AddRegisteredUnlocks();

            if (Testing.doMachineTest) {
                ChestDefinition voidChestDefinition = (ChestDefinition)ModUtils.GetResourceInfoByName("Void Chest");
                voidChestDefinition.inventorySizes = new List<Vector2Int>() { new Vector2Int(1, 1) };
                voidChestDefinition.invSizeOutput = new Vector2Int(1, 1);
            }
        }

        private void OnSaveStateLoaded(object sender, EventArgs e) {
            EMUAdditions.CustomData.Load(SaveState.instance.metadata.worldName);
            RecipeAdder.FetchUnlocks();
            ResourceAdder.FetchUnlocks();
            MachineAdder.FetchUnlocks();
        }

        private void OnGameSaved(object sender, EventArgs e) {
            EMUAdditions.CustomData.Save(sender.ToString());
        }

        // Internal Functions

        internal static void LogInfo(string message) {
            Log.LogInfo(message);
        }

        internal static void LogWarning(string message) {
            Log.LogWarning(message);
        }

        internal static void LogError(string message) {
            Log.LogError(message);
        }

        internal static bool IsTranslatableStringUnique(string input) {
            return !customTranslations.ContainsValue(input);
        }

        // Private Functions

        private void ApplyPatches() {
            Harmony.CreateAndPatchAll(typeof(FlowManagerPatch));
            Harmony.CreateAndPatchAll(typeof(GameDefinesPatch));
            Harmony.CreateAndPatchAll(typeof(LocsUtilityPatch));
        }

        internal static void SaveIdHistory() {
            Directory.CreateDirectory(dataFolder);
            List<string> fileLines = new List<string>();
            foreach(KeyValuePair<string, int> pair in idHistory) {
                fileLines.Add($"{pair.Key}|{pair.Value}");
            }

            string saveFile = $"{dataFolder}/Id History.txt";
            File.WriteAllLines(saveFile, fileLines);
        }

        private void LoadIdHistory() {
            string saveFile = $"{dataFolder}/Id History.txt";
            if(!File.Exists(saveFile)) {
                LogWarning($"No Id History save file found");
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
