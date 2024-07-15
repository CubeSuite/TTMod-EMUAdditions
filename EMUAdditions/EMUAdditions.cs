using EquinoxsModUtils.Additions.ContentAdders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EquinoxsModUtils.Additions
{
    public static partial class EMUAdditions
    {
        /// <summary>
        /// Registers a new Unlock to be added to the TechTree at the correct time.
        /// </summary>
        /// <param name="details">Details of the new Unlock. Ensure that all are provided.</param>
        /// <param name="shouldLog">Whether [EMUAdditions] Info messages should be logged for this call</param>
        public static void AddNewUnlock(NewUnlockDetails details, bool shouldLog = false) {
            if (!details.Validate()) {
                EMUAdditionsPlugin.LogError($"Abandoning attempt to add new Unlock '{details.displayName}'");
                return;
            }

            Unlock unlock = details.ConvertToUnlock();
            UnlockAdder.unlocksToAdd.Add(unlock);

            if (shouldLog) EMUAdditionsPlugin.LogInfo($"Successfully registered new Unlock '{details.displayName}' for adding to game");
        }

        /// <summary>
        /// Registers a new Resource with the provided details to be added to the game at the correct time.
        /// </summary>
        /// <param name="details">Container for the details of your new resource</param>
        /// <param name="shouldLog">Whether [EMUAdditions] Info messages should be logged for this call</param>
        public static void AddNewResource(NewResourceDetails details, bool shouldLog = false) {
            if (!details.Validate()) {
                EMUAdditionsPlugin.LogError($"Abandoning attempt to add new Resource '{details.name}'");
                return;
            }

            ResourceAdder.resourcesToAdd.Add(details);

            if (shouldLog) EMUAdditionsPlugin.LogInfo($"Successfully registered new Resource '{details.name}' for adding to game");
        }

        /// <summary>
        /// Requests EMU to create a SchematicsRecipeData with the provided details at the correct time.
        /// </summary>
        /// <param name="details">Container for the details of your new recipe</param>
        /// <param name="shouldLog">Whether an [EMUAdditions] Info message should be logged if recipe is valid</param>
        public static void AddNewRecipe(NewRecipeDetails details, bool shouldLog = false) {
            if (!details.Validate()) {
                EMUAdditionsPlugin.LogError($"Abandoning attempt to add new Recipe");
                return;
            }

            RecipeAdder.recipesToAdd.Add(details);

            if (shouldLog) EMUAdditionsPlugin.LogInfo($"Successfully registered new Recipe '{details.GetUniqueName()}' for adding to game");
        }

        /// <summary>
        /// Adds a new SchematicsSubHeader with the details provided in the arguments.
        /// </summary>
        /// <param name="title">The title of the new SchematicsSubHeader</param>
        /// <param name="parentTitle">The parent SchematicsHeader this should appear under</param>
        /// <param name="priority">Controls where the sub-category should be placed</param>
        /// <param name="shouldLog">Whether an [EMUAdditions] Info message should be logged on success</param>
        public static void AddNewSchematicsSubHeader(string title, string parentTitle, int priority, bool shouldLog = false) {
            if (string.IsNullOrEmpty(title)) {
                EMUAdditionsPlugin.LogError($"New SchematicsSubHeader title cannot be null or empty");
                return;
            }

            if (string.IsNullOrEmpty(parentTitle)) {
                EMUAdditionsPlugin.LogError($"New SchematicsSubHeader '{title}' parentTitle cannot be null or empty");
                return;
            }

            SchematicsSubHeader subHeader = (SchematicsSubHeader)ScriptableObject.CreateInstance(typeof(SchematicsSubHeader));
            subHeader.title = $"{title}";
            subHeader.priority = priority;
            SubHeaderAdder.subHeadersToAdd.Add(subHeader);
            SubHeaderAdder.parents.Add(parentTitle);

            if (shouldLog) EMUAdditionsPlugin.LogInfo($"Successfully registered new SchematicsSubHeader '{parentTitle}/{title}' to be added to game");
        }

        /// <summary>
        /// Registers a new instance of a MachineDefinition to be added to the game at the correct time.
        /// </summary>
        /// <typeparam name="T">MachineInstance derived class for the new machine</typeparam>
        /// <typeparam name="V">MachineDefinition derived class for the new machine</typeparam>
        /// <param name="details">Container for the details of your new machine.</param>
        /// <param name="shouldLog">Whether an [EMUAdditions] Info message should be logged on success</param>
        public static void AddNewMachine<T, V>(MachineDefinition<T,V> definition, NewResourceDetails details, bool shouldLog = false) where T : struct, IMachineInstance<T, V> where V : MachineDefinition<T, V> {
            if (!details.Validate()) {
                EMUAdditionsPlugin.LogError($"Abandoning attempt to add new Machine '{details.name}'");
                return;
            }

            definition.rawName = details.name;

            MachineAdder.machinesToAdd.Add(definition);
            MachineAdder.details.Add(details);

            if (shouldLog) EMUAdditionsPlugin.LogInfo($"Successfully registered new Machine '{definition.displayName}' for adding to game");
        }
    }
}
