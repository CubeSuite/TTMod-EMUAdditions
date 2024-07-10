using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriangleNet;

namespace EMUAdditions
{
    public static partial class EMUAdditions 
    {
        public static class CustomData 
        {
            // Objects & Variables
            private static Dictionary<string, object> customMachineData = new Dictionary<string, object>();
            private static bool hasCustomMachineDataLoaded = false;
            private static string dataFolder => EMUAdditionsPlugin.dataFolder;

            // Public Functions

            /// <summary>
            /// Adds a custom member for an instance of a machine if it has not already been added. See repo README for explanation.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="instanceId">The instanceId of the target machine.</param>
            /// <param name="name">The name of the new member.</param>
            /// <param name="value">The value of the new member.</param>
            public static void Add<T>(uint instanceId, string name, T value) {
                List<string> acceptableTypes = new List<string>() {
                    typeof(uint).ToString(),
                    typeof(int).ToString(),
                    typeof(float).ToString(),
                    typeof(double).ToString(),
                    typeof(bool).ToString(),
                    typeof(string).ToString(),
                    typeof(char).ToString(),
                };
                if (!acceptableTypes.Contains(typeof(T).ToString())) {
                    EMUAdditionsPlugin.Log.LogError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
                }

                string key = $"{instanceId}-{name}-{typeof(T)}";
                if (!customMachineData.ContainsKey(key)) {
                    customMachineData.Add(key, value);
                }
            }

            /// <summary>
            /// Sets the value of a custom member for an instance of a machine. See repo README for explanation.
            /// </summary>
            /// <typeparam name="T">The type of the member. See repo README for acceptable types.</typeparam>
            /// <param name="instanceId">The instanceId of the target machine.</param>
            /// <param name="name">The name of the new member.</param>
            /// <param name="value">The value of the new member.</param>
            public static void Update<T>(uint instanceId, string name, T value) {
                List<string> acceptableTypes = new List<string>() {
                    typeof(uint).ToString(),
                    typeof(int).ToString(),
                    typeof(float).ToString(),
                    typeof(double).ToString(),
                    typeof(bool).ToString(),
                    typeof(string).ToString(),
                    typeof(char).ToString(),
                };
                if (!acceptableTypes.Contains(typeof(T).ToString())) {
                    EMUAdditionsPlugin.Log.LogError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
                }

                string key = $"{instanceId}-{name}-{typeof(T)}";
                if (!customMachineData.ContainsKey(key)) {
                    EMUAdditionsPlugin.Log.LogWarning($"Custom data with key '{key}' has not been added for machine yet, adding instead of updating.");
                    Add(instanceId, name, value);
                    return;
                }

                customMachineData[key] = value;
            }

            /// <summary>
            /// Gets the value of a custom member for an instance of a machine. See repo README for exlpanation.
            /// </summary>
            /// <typeparam name="T">The type of the member.</typeparam>
            /// <param name="instanceId">The instanceId of the target machine.</param>
            /// <param name="name">The name of the new member.</param>
            /// <returns>The value of the new member if successful, default(T) otherwise.</returns>
            public static T Get<T>(uint instanceId, string name) {
                if (!hasCustomMachineDataLoaded) {
                    EMUAdditionsPlugin.Log.LogError($"GetCustomDataForMachine() called before custom data has loaded.");
                    EMUAdditionsPlugin.Log.LogInfo($"Try using the SaveStateLoaded event or hasSaveStateLoaded variable");
                    return default;
                }

                List<string> acceptableTypes = new List<string>() {
                    typeof(uint).ToString(),
                    typeof(int).ToString(),
                    typeof(float).ToString(),
                    typeof(double).ToString(),
                    typeof(bool).ToString(),
                    typeof(string).ToString(),
                    typeof(char).ToString(),
                };
                if (!acceptableTypes.Contains(typeof(T).ToString())) {
                    EMUAdditionsPlugin.Log.LogError($"EMU cannot save custom data of type '{typeof(T)}', please use one of: uint, int, float, double, bool, string, char");
                }

                string key = $"{instanceId}-{name}-{typeof(T)}";
                if (customMachineData.ContainsKey(key)) {
                    return (T)customMachineData[key];
                }
                else {
                    EMUAdditionsPlugin.Log.LogError($"Could not find custom data with key '{key}'");
                    return default;
                }
            }

            /// <summary>
            /// Checks if any custom data exists for the machine instanceId provided in the argument.
            /// </summary>
            /// <param name="instanceId">The instanceId of the machine.</param>
            /// <returns>true if data is found, false if not</returns>
            public static bool AnyExists(uint instanceId) {
                foreach (string key in customMachineData.Keys) {
                    if (key.Split('-')[0] == instanceId.ToString()) return true;
                }

                return false;
            }

            /// <summary>
            /// Checks if the specified custom data field exists for the machine instanceId provided in the argument.
            /// </summary>
            /// <param name="instanceId">The instanceId of the machine.</param>
            /// /// <param name="name">The name of the new member.</param>
            /// <returns>true if data is found, false if not</returns>
            public static bool FieldExists<T>(uint instanceId, string name) {
                string key = $"{instanceId}-{name}-{typeof(T)}";
                return customMachineData.ContainsKey(key);
            }

            // Internal Functions

            internal static void Save(string worldName) {
                List<string> fileLines = new List<string>();
                foreach (KeyValuePair<string, object> dataPair in customMachineData) {
                    fileLines.Add($"{dataPair.Key}|{dataPair.Value}");
                }

                Directory.CreateDirectory(dataFolder);
                string saveFile = $"{dataFolder}/{worldName} CustomData.txt";
                File.WriteAllLines(saveFile, fileLines);
            }

            internal static void Load(string worldName) {
                string saveFile = $"{dataFolder}/{worldName} CustomData.txt";
                if (!File.Exists(saveFile)) {
                    EMUAdditionsPlugin.LogWarning($"No CustomData save file found for world '{worldName}'");
                    return;
                }

                string[] fileLines = File.ReadAllLines(saveFile);
                foreach (string line in fileLines) {
                    string[] parts = line.Split('|');
                    string valueString = parts[1];
                    object value = null;

                    string key = parts[0];
                    string[] keyParts = key.Split('-');
                    string type = keyParts[2];

                    switch (type) {
                        case "System.UInt32": value = uint.Parse(valueString); break;
                        case "System.Int32": value = int.Parse(valueString); break;
                        case "System.Single": value = float.Parse(valueString); break;
                        case "System.Double": value = double.Parse(valueString); break;
                        case "System.Boolean": value = bool.Parse(valueString); break;
                        case "System.String": value = valueString; break;
                        case "System.Char": value = char.Parse(valueString); break;
                        default:
                            EMUAdditionsPlugin.LogError($"Cannot load custom data (key: '{key}') with unhandled type: '{type}'");
                            continue;
                    }

                    customMachineData[key] = value;
                }

                hasCustomMachineDataLoaded = true;
                EMUAdditionsPlugin.LogInfo("Loaded custom machine data");
            }
        }
    }
}
