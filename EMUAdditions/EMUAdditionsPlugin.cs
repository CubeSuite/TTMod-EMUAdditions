using BepInEx;
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
using EquinoxsModUtils.Additions;

namespace EquinoxsModUtils
{
    internal static class Testing 
    {
        internal static bool doUnlockTest = false;
        internal static bool doResourcePlusTest = false;
        internal static bool doMachineTest = false;

        internal static void DoTests() {
            if (doUnlockTest) {
                EMUAdditions.AddNewUnlock(new NewUnlockDetails() {
                    displayName = "My New Unlock",
                    description = "Description of my new Unlock",
                    category = Unlock.TechCategory.Terraforming,
                    requiredTier = TechTreeState.ResearchTier.Tier2,
                    coreTypeNeeded = ResearchCoreDefinition.CoreType.Purple,
                    coreCountNeeded = 20,
                    treePosition = 10,
                });
            }

            if (doResourcePlusTest) {
                EMUAdditions.AddNewResource(new NewResourceDetails() {
                    parentName = EMU.Names.Resources.Limestone,

                    name = "Limestone 3",
                    description = "Limestone 3 Test",
                    headerTitle = "Intermediates",
                    subHeaderTitle = "Test Parts 2",
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    fuelAmount = 100f,
                    sortPriority = 10,
                    unlockName = EMU.Names.Unlocks.CoreComposer,
                });
                EMUAdditions.AddNewRecipe(new NewRecipeDetails() {
                    GUID = EMUAdditionsPlugin.MyGUID,
                    craftingMethod = CraftingMethod.Assembler,
                    craftTierRequired = 0,
                    duration = 0.1f,
                    ingredients = new List<RecipeResourceInfo>() {
                        new RecipeResourceInfo() {
                            name = EMU.Names.Resources.Limestone,
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
                    unlockName = EMU.Names.Unlocks.CoreComposer
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
                    unlockName = EMU.Names.Unlocks.BasicLogistics,
                    parentName = EMU.Names.Resources.Container
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
                            name = EMU.Names.Resources.IronIngot,
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
                    unlockName = EMU.Names.Unlocks.BasicLogistics
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
        private const string VersionString = "2.0.1";

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

            EMU.Events.GameDefinesLoaded += OnGameDefinesLoaded;
            EMU.Events.SaveStateLoaded += OnSaveStateLoaded;
            EMU.Events.GameSaved += OnGameSaved;

            Testing.DoTests();

            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
        }

        // Events

        private void OnGameDefinesLoaded() {
            UnlockAdder.AddRegisteredUnlocks();

            if (Testing.doUnlockTest) {
                Unlock myUnlock = EMU.Unlocks.GetUnlockByName("My New Unlock");
                Unlock excavatorBit = EMU.Unlocks.GetUnlockByName(EMU.Names.Unlocks.ExcavatorBitMKI);
                Unlock bricks = EMU.Unlocks.GetUnlockByName(EMU.Names.Unlocks.BasicPowderBricks);

                myUnlock.requiredTier = excavatorBit.requiredTier;
                myUnlock.treePosition = bricks.treePosition;

                ResourceInfo limestone = EMU.Resources.GetResourceInfoByName(EMU.Names.Resources.Limestone);
                myUnlock.sprite = EMU.Images.LoadSpriteFromFile("EMUAdditions.Images.VoidChest.png");
            }

            if (Testing.doMachineTest) {
                ChestDefinition voidChestDefinition = (ChestDefinition)EMU.Resources.GetResourceInfoByName("Void Chest");
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
            Harmony.CreateAndPatchAll(typeof(TechActivatedSystemMessage));
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
