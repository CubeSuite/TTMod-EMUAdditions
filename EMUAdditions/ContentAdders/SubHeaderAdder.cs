using EquinoxsModUtils.Additions.Patches;
using EquinoxsModUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions.ContentAdders
{
    internal static class SubHeaderAdder
    {
        // Objects & Variables
        internal static List<SchematicsSubHeader> subHeadersToAdd = new List<SchematicsSubHeader>();
        internal static List<string> parents = new List<string>();
        internal static Dictionary<string, int> idHistory = new Dictionary<string, int>();
        private static string dataFolder => EMUAdditionsPlugin.dataFolder;

        // Internal Functions

        internal static void AddRegisteredSubHeaders() {
            EMUAdditionsPlugin.LogInfo($"{subHeadersToAdd.Count} new SchematicsSubHeaders have been registered for adding");

            List<SchematicsSubHeader> addedBefore = subHeadersToAdd.Where(subHeader => idHistory.ContainsKey(subHeader.title)).ToList();
            foreach(SchematicsSubHeader subHeader in addedBefore) {
                EMUAdditionsPlugin.LogInfo($"Trying to add historic new SchematicsSubHeader '{subHeader.title}'");
                
                subHeader.uniqueId = idHistory[subHeader.title];
                if (AddSubHeaderToGame(subHeader)) {
                    EMUAdditionsPlugin.LogInfo($"Added historic new SchematicsSubHeader '{subHeader.title}' to the game with id {subHeader.uniqueId}");
                }
            }
            
            List<SchematicsSubHeader> neverAdded = subHeadersToAdd.Where(subHeader => !idHistory.ContainsKey(subHeader.title)).ToList();
            foreach(SchematicsSubHeader subHeader in neverAdded) {
                EMUAdditionsPlugin.LogInfo($"Trying to add brand new SchematicsSubHeader '{subHeader.title}'");
                
                subHeader.uniqueId = GetNewSubHeaderID();
                idHistory.Add(subHeader.title, subHeader.uniqueId);
                if (AddSubHeaderToGame(subHeader)) {
                    EMUAdditionsPlugin.LogInfo($"Added brand new SchematicsSubHeader '{subHeader.title}' to the game with id {subHeader.uniqueId}");
                }
            }

            SaveIdHistory();
        }

        // Private Functions

        private static bool AddSubHeaderToGame(SchematicsSubHeader subHeader) {
            int index = subHeadersToAdd.IndexOf(subHeader);
            string parentTitle = parents[index];

            subHeader.filterTag = ModUtils.GetSchematicsHeaderByTitle(parentTitle);
            if (subHeader.filterTag == null) {
                EMUAdditionsPlugin.LogError($"Aborting attempt to add new SchematicsSubHeader '{subHeader.title}'");
                return false;
            }

            string titleHash = LocsUtility.GetHashString(subHeader.title);
            EMUAdditionsPlugin.customTranslations[titleHash] = subHeader.title;

            GameDefines.instance.schematicsSubHeaderEntries.Add(subHeader);
            return true;
        }

        private static int GetNewSubHeaderID() {
            int max = 0;
            foreach(SchematicsSubHeader subHeader in GameDefines.instance.schematicsSubHeaderEntries) {
                if (subHeader.uniqueId > max) max = subHeader.uniqueId;
            }

            foreach(int id in idHistory.Values) {
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

            string saveFile = $"{dataFolder}/SubHeader Id History.txt";
            File.WriteAllLines(saveFile, filesLines);
        }

        internal static void LoadIdHistory() {
            string saveFile = $"{dataFolder}/SubHeader Id History.txt";
            if (!File.Exists(saveFile)) {
                EMUAdditionsPlugin.LogWarning($"No SubHeader Id History save file found");
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
