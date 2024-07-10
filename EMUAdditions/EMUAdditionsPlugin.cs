using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using EMUAdditions.ContentAdders;
using EMUAdditions.Patches;
using EquinoxsModUtils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EMUAdditions
{
    internal static class Testing 
    {
        internal static bool doUnlockTest = false;
        internal static bool doResourcePlusTest = false;
        internal static bool doMachineTest = true;

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
        private const string VersionString = "1.0.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        internal static ManualLogSource Log = new ManualLogSource(PluginName);

        // Objects & Variables
        internal static Dictionary<string, string> customTranslations = new Dictionary<string, string>();
        internal static string dataFolder = $"{Application.persistentDataPath}/EMUAdditions";

        // Unity Functions

        private void Awake() {
            Log = Logger;
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();

            UnlockAdder.LoadIdHistory();
            ResourceAdder.LoadIdHistory();
            RecipeAdder.LoadIdHistory();
            SubHeaderAdder.LoadIdHistory();
            MachineAdder.LoadIdHistory();

            ApplyPatches();

            ModUtils.GameDefinesLoaded += OnGameDefinesLoaded;
            ModUtils.SaveStateLoaded += OnSaveStateLoaded;
            ModUtils.GameLoaded += OnGameLoaded;
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
                voidChestDefinition.invSizeOutput = new Vector2Int(1, 1);T 
            }
        }

        private void OnSaveStateLoaded(object sender, EventArgs e) {
            EMUAdditions.CustomData.Load(SaveState.instance.metadata.worldName);
        }

        private void OnGameLoaded(object sender, EventArgs e) {
            
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
            Harmony.CreateAndPatchAll(typeof(GameDefinesPatch));
            Harmony.CreateAndPatchAll(typeof(LocsUtilityPatch));
        }
    }
}
