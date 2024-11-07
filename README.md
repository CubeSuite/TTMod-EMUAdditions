# Contents

- [Contents](#contents)
- [EMU Additions (EMUA)](#emu-additions-emua)
  - [How To Use EMUA](#how-to-use-emua)
  - [Adding Content](#adding-content)
    - [Details Classes](#details-classes)
      - [NewUnlockDetails](#newunlockdetails)
      - [NewResourceDetails](#newresourcedetails)
      - [NewRecipeDetails](#newrecipedetails)
    - [Functions](#functions)
      - [AddNewUnlock](#addnewunlock)
      - [AddNewResource](#addnewresource)
      - [AddNewRecipe](#addnewrecipe)
      - [AddNewSchematicsSubHeader](#addnewschematicssubheader)
      - [AddNewMachine](#addnewmachine)
      - [AddNewEquipment](#addnewequipment)
  - [Custom Machine Data](#custom-machine-data)
    - [Add](#add)
    - [Update](#update)
    - [Get](#get)
    - [AnyExists](#anyexists)
    - [FieldExists](#fieldexists)


# EMU Additions (EMUA)

**See the [Wiki Tab](https://github.com/CubeSuite/TTMod-EMUAdditions/wiki) for full tutorials.**

EMUAdditions is a library that allows modders to add new content to the game. If you are planning to add content, you must use this library or you could easily break player's saves. 

## How To Use EMUA

1. Download and install EMUA from Thunderstore like any other mod.
1. In Visual Studio 22, in the Solution Explorer, right click 'References'
1. Click 'Add Reference'
1. Navigate to 'Techtonica/BepInEx/plugins/EMUAdditions/EMUAdditions.dll'

## Adding Content

EMUA features several functions that allow you to add new content to the game. These functions should be called during your mod's `Awake()` function. Each of them takes an instance of a `New...Details` class that contains all the info EMUA needs to add your new content. This information is stored for later use and the content is added to the game at the correct time.

### Details Classes

#### NewUnlockDetails

This class contains the details required to add a new `Unlock` to the game. 

**Note:** All fields except `dependencyNames` and `sprite` are mandatory. If you leave `sprite` as `null`, a default icon will be displayed for the `Unlock` in the Tech Tree. You can use [Equinox's Mod Utils](https://thunderstore.io/c/techtonica/p/Equinox/EquinoxsModUtils/) to update this sprite to either a new one from file or an existing in-game sprite.

```csharp
public class NewUnlockDetails {
    public string displayName;
    public string description;
    public Unlock.TechCategory category;
    public TechTreeState.ResearchTier requiredTier;
    public ResearchCoreDefinition.CoreType coreTypeNeeded;
    public int coreCountNeeded;
    public int treePosition;
    public List<string> dependencyNames = new List<string>();
    public Sprite sprite;
}
```
---
#### NewResourceDetails

This class contains the details required to add a new `ResourceInfo` to the game. As machines inherit from `ResourceInfo`, this class is also used to add new machines to the game. The `ResourceInfo` defined by the `parentName` field is cloned and updated with members from this class. See the [Wiki Tab](https://github.com/CubeSuite/TTMod-EMUAdditions/wiki) for more info.

**Note:** All fields except `fuelAmount` are mandatory. 

```csharp
public class NewResourceDetails {
    public string parentName;

    public string name;
    public string description;
    public CraftingMethod craftingMethod;
    public int craftTierRequired;
    public float fuelAmount;
    public string headerTitle;
    public string subHeaderTitle;
    public int maxStackCount = 500;
    public Sprite sprite;
    public int sortPriority;
    public string unlockName;
}
```
---
#### NewRecipeDetails

This class contains the details required to add a new `SchematicsRecipeData` to the game. 

**Note:** All fields are mandatory.

```csharp
public class NewRecipeDetails{
    public string GUID;
    public CraftingMethod craftingMethod;
    public int craftTierRequired;
    public float duration;
    public List<RecipeResourceInfo> ingredients = new List<RecipeResourceInfo>();
    public List<RecipeResourceInfo> outputs = new List<RecipeResourceInfo>();
    public int sortPriority;
    public string unlockName;
}

public struct RecipeResourceInfo
{
    public string name;
    public int quantity;
}
```
---
### Functions

#### AddNewUnlock
```csharp
public static void AddNewUnlock(
    NewUnlockDetails details,
    bool shouldLog = false
)
```

Registers a new `Unlock` to be added to the game at the correct time. Additional Info messages are logged when `shouldLog` is true.

---
#### AddNewResource

```csharp
public static void AddNewResource(
    NewResourceDetails details,
    bool shouldLog = false
)
```

Registers a new `ResourceInfo` (item) to be added to the game at the correct time. Additional Info messages are logged when `shouldLog` is true.

---
#### AddNewRecipe
```csharp
public static void AddNewRecipe(
    NewRecipeDetails details,
    bool shouldLog = false
)
```

Registers a new `SchematicsRecipeData` (recipe) to be added to the game at the correct time. Additional Info messages are logged when `shouldLog` is true.

---
#### AddNewSchematicsSubHeader
```csharp
public static void AddNewSchematicsSubHeader(
    string title,
    string parentTitle,
    int priority,
    bool shouldLog = false
)
```

Registers a new `SchematicsSubHeader` to be added to the game at the correct time. This is the sub category that appears under each header (e.g. Production, Logistics). Priority controls where the new category appears in the crafting menu.

---
#### AddNewMachine
```csharp
public static void AddNewMachine<T, V>(
    MachineDefinition<T, V> definition,
    NewResourceDetails details,
    bool shouldLog = false
)
```

Read the [Adding A New Machine](missinglink) tutorial for this one.

---
#### AddNewEquipment
```csharp
public static void AddNewEquipment<T>(
    Equipment equipment,
    NewResourceDetails details,
    bool shouldLog = false
)
```
Read the [Adding New Equipment](missinglink) tutorial for this one.

---
## Custom Machine Data

Custom data is designed to allow you to essentially add fields to existing machine instances. For example, in my [Smart Inserters Mod](https://thunderstore.io/c/techtonica/p/Equinox/SmartInserters/), I need to keep track of each inserter's limit. The most ideal to solution to this would be adding an `int limit` field to `InserterInstance`, but this isn't possible, so Custom Machine Data is the next best thing. For more info on how to use it, see the [Wiki Tab](https://github.com/CubeSuite/TTMod-EMUAdditions/wiki). EMUA will handle saving and loading this custom data from file for you.

---
### Add

Deprecated, use [Update()](#update) instead.

---
### Update

```csharp
public static void Update<T>(
    uint instanceId,
    string name,
    T value
)
```

Sets the value of the named field of custom data for the given machine id.

---

### Get

```csharp
public static Get<T>(
    uint instanceId, 
    string name
)
```

Gets the value of the named field of custom data for the given machine id.

---
### AnyExists

```csharp
public static bool AnyExists(
    uint instanceId
)
```

Returns `true` if any fields of Custom Data exist for the given machine id.

---
### FieldExists

```csharp
public static bool FieldExists<T>(
    uint instanceId,
    string name
)
```

Returns `true` if the named field exists for the given machine id.