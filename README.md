
# Anything Anywhere Config List


## Table of Contents
- [Placing](#placing)
- [Building](#building)
- [Farming](#farming)
- [Miscellaneous](#miscellaneous)
- [Debug Commands](#debug-commands)

## Placing
- [Enable Placing](#enable-placing)
- [Place Anywhere](#place-anywhere)
- [Rug Removal Bypass](#rug-removal-bypass)
- [Wall Furniture Indoors](#wall-furniture-indoors)

##

##### `Enable Placing`
This toggle enables the placement portion of the mod. When enabled you can play objects (chests, mini-fridges, fences) and furniture on any free tile.\
This also enables infinite reach when placing furniture in all locations so you can easily rearrange furniture.

##### `Place Anywhere`
This toggle enables placing objects and furniture inside of walls or any other locked tile with two exceptions.
* You cannot place objects on top of another object
* You cannot place objects or furniture inside of the mines

##### `Rug Removal Bypass`
This toggle disables the check that makes sure rugs don't have anything on top of them when picking them up.\
Its intended use is if for some reason you cannot pick up a rug you can disable the checks.

##### `Wall Furniture Indoors`
This enables placing wall furniture anywhere indoors. The default is off so decorating is easier indoors but if you want a window on your floor feel free to enable it.

## Building
- [Enable Building](#enable-building)
- [Build Anywhere](#build-anywhere)
- [Build Free & Instantly](#build-free--instantly)
- [Build Anywhere Menu](#build-anywhere-menu)
- [Wizard Build Menu](#wizard-build-menu)
- [Building Modifier Key](#building-modifier-key)
- [Enable Greenhouse](#enable-greenhouse)
- [Remove Build Conditions](#remove-build-conditions)
- [Build Indoors](#build-indoors)
- [Magic Ink Bypass](#magic-ink-bypass)

##

##### `Enable Building`
This toggle enables all the building and menu features of the mod.

##### `Build Anywhere`
This toggle makes all tiles buildable, enabling you to build in walls or have buildings intersect each other.

##### `Build Free & Instantly`
This toggle will set the costs of all blueprints to be free and will make buildings build instantly.

##### `Build Anywhere Menu`
Opens up the carpenter menu anywhere. The viewport will be centered on the player.

##### `Wizard Build Menu`
Opens up the Wizards build menu anywhere. The viewport will be centered on the player.

##### `Building Modifier Key`
When this key is held down while you are building, upgrading, or destroying buildings, you will not be kicked out of the menu.\
This lets you build or destroy buildings really fast, no more waiting 15 seconds each time you want to build multiple fish ponds.\
Building or upgrading multiple buildings will still consume resources, you will be kicked out of the menu if you run out. 

##### `Enable Greenhouse`
This toggle adds the Greenhouse as a blueprint that you can build. To unlock the blueprint you need to have the Greenhouse unlocked in the Community Center or the Joja path.\
The blueprint costs:
* 150,000 Gold
* 100 Hardwood
* 20 Refined Quartz
* 10 Iridium Bars

##### `Remove Build Conditions`
This toggle removes the build conditions on all blueprints. This lets you build things like the Island Obelisk without ever visiting the island.\
This works with mods that add buildings locked behind certain events too.

##### `Build Indoors`
This toggle enables building structures indoors. This can lead to errors.\
For example entering a building that you built inside of the coop will soft lock the game, requiring a restart.\
Building inside of the farmhouse and greenhouse does work though. If you have the Farmhouse Fixes mod make sure you enable `Non-Hardcoded Warps` in that mod.

##### `Magic Ink Bypass`
This toggle skips the check for magic ink when opening the Wizards build menu.\
This only works for this mods Wizards build menu, you sill need magic ink to open the Wizards menu at the tower.

##### `Hide Location`
This button will add the current player location to a list to not display in Robins build menu.\
When opening up the build menu the map properties "AlwaysActive" and "CanBuildHere" are set to true for the current location.\
To remove non blacklisted locations with no buildings from Robins menu, reload your save.

## Farming
- [Enable Farming](#enable-farming)
- [Hoe Anything](#hoe-anything)
- [Fruit Tree Tweaks](#fruit-tree-tweaks)
- [Wild Tree Tweaks](#wild-tree-tweaks)

##

##### `Enable Farming`
This toggle marks all locations as plantable, and makes most dirt tiles hoeable.\
This enables farming in all locations with dirt.\
Note that some dirt tiles added in 1.6 aren't labeled as dirt, and thus cannot be hoed, a simple fix for this is to enable the `Hoe Anything` toggle.

##### `Hoe Anything`
This toggle will let the player hoe any tile. This will make farming on stone or other non-dirt tiles possible. 

##### `Fruit Tree Tweaks`
This toggle will remove the placement and growth restrictions on fruit trees.\
This will let you plant them as close together as you want and the growth won't be blocked by other trees or walls

##### `Wild Tree Tweaks`
This toggle will remove the placement and growth restrictions on wild trees.\
This includes things like acorn, pine, maple, and mahogany seeds.\
Enabling this WILL make wild trees quickly grow out of control and you will be taken over by the forest. 

## Miscellaneous
- [Animal Relocation](#animal-relocation)
- [Cask Tweaks](#cask-tweaks)
- [Jukebox Tweaks](#jukebox-tweaks)
- [Gold Clock Tweaks](#gold-clock-tweaks)
- [Multiple Mini-Obelisk](#multiple-mini-obelisk)
- [House/Cabin Bypass](#housecabin-bypass)

##

##### `Animal Relocation`
This toggle will enable the animal relocation menu.\
This is here so you can relocate animals to any location that has a building for them.\
If disabled animals can only be relocated to the main farm.

##### `Cask Tweaks`
This toggle will let the player use the cask outside of the cellar.\
Its disabled by default as it can be seen as 'cheating' by some players.

##### `Jukebox Tweaks`
This toggle enables using the jukebox in all locations it can be placed.\
The jukebox does not work outside while it is raining.

##### `Gold Clock Tweaks`
This toggle lets the gold clock building work in any location, rather than just the farm.

##### `Multiple Mini-Obelisk`
This toggle lets you place more than two mini-obelisks per location.\
Having more than two per location will make the mini-obelisks not work as intended, but it can be used as decoration.\
If you have the Multiple Mini-Obelisk mod, this toggle will be enabled by default, otherwise it is disabled by default.


## Debug Commands

These commands are executed in the SMAPI console. Use with caution and only if you understand their effects.

- `aa_remove_objects`
  Removes all objects with a specified ID at a specified location.
  
  Usage: 
    `aa_remove_objects [LOCATION] [OBJECT_ID]`
  
  Example:
    `aa_remove_objects current (O)499`

- `aa_remove_furniture`
  Removes all furniture with a specified ID at a specified location.
  
  Usage:
    `aa_remove_furniture [LOCATION] [FURNITURE_ID]`
  
  Example:
    `aa_remove_furniture current (F)1371`

- `aa_active`
  Prints a list of all the locations that are set to AlwaysActive
  
  Usage:
    `aa_active`

