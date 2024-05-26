global using SObject = StardewValley.Object;
using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using AnythingAnywhere.Framework;
using AnythingAnywhere.Framework.Interfaces;
using AnythingAnywhere.Framework.Patches.Menus;
using AnythingAnywhere.Framework.Patches.Locations;
using AnythingAnywhere.Framework.Patches.GameLocations;
using AnythingAnywhere.Framework.Patches.StandardObjects;
using AnythingAnywhere.Framework.Patches.TerrainFeatures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Common.Managers;
using Common.Helpers;
using Common.Util;
using System.Linq;
using System;

namespace AnythingAnywhere
{
    public class ModEntry : Mod
    {
        internal static IModHelper ModHelper { get; set; }
        internal static IMonitor ModMonitor { get; set; }
        internal static ModConfig Config { get; set; }
        internal static Multiplayer Multiplayer { get; set; }
        internal static ApiManager ApiManager { get; set; }
        internal static ICustomBushApi CustomBushApi { get; set; }
        internal static EventHandlers EventHandlers { get; set; }

        private static Harmony harmony;

        public override void Entry(IModHelper helper)
        {
            // Setup the monitor, helper, config and multiplayer
            ModMonitor = Monitor;
            ModHelper = helper;
            Config = Helper.ReadConfig<ModConfig>();
            Multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            
            // Setup the managers/handlers
            ApiManager = new ApiManager(helper, ModMonitor);
            EventHandlers = new EventHandlers();

            // Load the Harmony patches
            harmony = new Harmony(this.ModManifest.UniqueID);

            // GameLocation
            new GameLocationPatch(harmony).Apply();

            // Location
            new FarmHousePatch(harmony).Apply();

            // Menu
            new CarpenterMenuPatch(harmony).Apply();
            new AnimalQueryMenuPatch(harmony).Apply();

            // StandardObject
            new CaskPatch(harmony).Apply();
            new FurniturePatch(harmony).Apply();
            new BedFurniturePatch(harmony).Apply();
            new MiniJukeboxPatch(harmony).Apply();
            new ObjectPatch(harmony).Apply();

            // TerrainFeature
            new FruitTreePatch(harmony).Apply();
            new TreePatch(harmony).Apply();

            // Add debug commands
            helper.ConsoleCommands.Add("aa_remove_objects", "Removes all objects of a specified ID at a specified location.\n\nUsage: aa_remove_objects [LOCATION] [OBJECT_ID]", this.DebugRemoveObjects);
            helper.ConsoleCommands.Add("aa_remove_furniture", "Removes all furniture of a specified ID at a specified location.\n\nUsage: aa_remove_furniture [LOCATION] [FURNITURE_ID]", this.DebugRemoveFurniture);
            helper.ConsoleCommands.Add("aa_active", "Lists all active locations.\n\nUsage: aa_active", this.DebugListActiveLocations);

            // Hook into Game events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += EventHandlers.OnSaveLoaded;
            helper.Events.World.BuildingListChanged += EventHandlers.OnBuildingListChanged;
            helper.Events.Input.ButtonsChanged += EventHandlers.OnButtonsChanged;
            helper.Events.Content.AssetRequested += EventHandlers.OnAssetRequested;
            helper.Events.GameLoop.UpdateTicked += EventHandlers.OnUpdateTicked;
            helper.Events.Player.Warped += EventHandlers.OnWarped;

            // Hook into Custom events
            ButtonOptions.Click += EventHandlers.OnClick;
            ConfigUtilities.ConfigChanged += EventHandlers.OnConfigChanged;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("furyx639.CustomBush"))
            {
                CustomBushApi = ApiManager.GetApi<ICustomBushApi>("furyx639.CustomBush");
            }

            if (Helper.ModRegistry.IsLoaded("PeacefulEnd.MultipleMiniObelisks"))
            {
                Config.MultipleMiniObelisks = true;
            }

            if (Helper.ModRegistry.IsLoaded("mouahrara.RelocateFarmAnimals"))
            {
                Config.EnableAnimalRelocate = false;
            }

            ConfigManager.Initialize(ModManifest, Config, ModHelper, ModMonitor, harmony);
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu"))
            {
                // Register the main page
                ConfigManager.AddPageLink("Placing");
                ConfigManager.AddPageLink("Building");
                ConfigManager.AddPageLink("Farming");
                ConfigManager.AddPageLink("Other");

                // Register the placing settings
                ConfigManager.AddPage("Placing");
                ConfigManager.AddButtonOption("Placing", "ResetPage", "Placing");
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddOption(nameof(ModConfig.EnablePlacing));
                ConfigManager.AddOption(nameof(ModConfig.EnableFreePlace));
                ConfigManager.AddOption(nameof(ModConfig.EnableWallFurnitureIndoors));
                ConfigManager.AddOption(nameof(ModConfig.EnableRugRemovalBypass));

                // Register the build settings
                ConfigManager.AddPage("Building");
                ConfigManager.AddButtonOption("Building", "ResetPage", "Building");
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddOption(nameof(ModConfig.EnableBuilding));
                ConfigManager.AddOption(nameof(ModConfig.EnableBuildAnywhere));
                ConfigManager.AddOption(nameof(ModConfig.EnableInstantBuild));
                ConfigManager.AddOption(nameof(ModConfig.EnableFreeBuild));
                ConfigManager.AddOption(nameof(ModConfig.BuildMenu));
                ConfigManager.AddOption(nameof(ModConfig.WizardBuildMenu));
                ConfigManager.AddOption(nameof(ModConfig.BuildModifier));
                ConfigManager.AddOption(nameof(ModConfig.EnableGreenhouse));
                ConfigManager.AddOption(nameof(ModConfig.RemoveBuildConditions));
                ConfigManager.AddOption(nameof(ModConfig.EnableBuildingIndoors));
                ConfigManager.AddOption(nameof(ModConfig.BypassMagicInk));
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddButtonOption("BlacklistedLocations", "BlacklistedLocations", "BlacklistCurrentLocation", afterReset);

                // Register the farming settings
                ConfigManager.AddPage("Farming");
                ConfigManager.AddButtonOption("Farming", "ResetPage", "Farming");
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddOption(nameof(ModConfig.EnablePlanting));
                ConfigManager.AddOption(nameof(ModConfig.EnableDiggingAll));
                ConfigManager.AddOption(nameof(ModConfig.EnableFruitTreeTweaks));
                ConfigManager.AddOption(nameof(ModConfig.EnableWildTreeTweaks));

                // Register the other settings
                ConfigManager.AddPage("Other");
                ConfigManager.AddButtonOption("Other", "ResetPage", "Other");
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddOption(nameof(ModConfig.EnableAnimalRelocate));
                ConfigManager.AddOption(nameof(ModConfig.EnableCaskFunctionality));
                ConfigManager.AddOption(nameof(ModConfig.EnableJukeboxFunctionality));
                ConfigManager.AddOption(nameof(ModConfig.EnableGoldClockAnywhere));
                ConfigManager.AddOption(nameof(ModConfig.MultipleMiniObelisks));
            }
        }

        private static readonly Action afterReset = () =>
        {
            EventHandlers.ResetBlacklist();
        };

        private void DebugRemoveFurniture(string command, string[] args)
        {
            if (args.Length <= 1)
            {
                Monitor.Log($"Missing required arguments: [LOCATION] [FURNITURE_ID]", LogLevel.Warn);
                return;
            }

            // check context
            if (!Context.IsWorldReady)
            {
                ModMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            // get target location
            var location = Game1.locations.FirstOrDefault(p => p.Name != null && p.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (location == null && args[0] == "current")
            {
                location = Game1.currentLocation;
            }
            if (location == null)
            {
                string[] locationNames = (from loc in Game1.locations where !string.IsNullOrWhiteSpace(loc.Name) orderby loc.Name select loc.Name).ToArray();
                ModMonitor.Log($"Could not find a location with that name. Must be one of [{string.Join(", ", locationNames)}].", LogLevel.Error);
                return;
            }

            // remove objects
            int removed = 0;
            foreach (var pair in location.furniture.ToArray())
            {
                if (pair.QualifiedItemId == args[1])
                {
                    location.furniture.Remove(pair);
                    removed++;
                }
            }

            ModMonitor.Log($"Command removed {removed} furniture objects at {location.NameOrUniqueName}", LogLevel.Info);
            return;
        }

        private void DebugRemoveObjects(string command, string[] args)
        {
            if (args.Length <= 1)
            {
                Monitor.Log($"Missing required arguments: [LOCATION] [OBJECT_ID]", LogLevel.Warn);
                return;
            }

            // check context
            if (!Context.IsWorldReady)
            {
                ModMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            // get target location
            var location = Game1.locations.FirstOrDefault(p => p.Name != null && p.Name.Equals(args[0], StringComparison.OrdinalIgnoreCase));
            if (location == null && args[0] == "current")
            {
                location = Game1.currentLocation;
            }
            if (location == null)
            {
                string[] locationNames = (from loc in Game1.locations where !string.IsNullOrWhiteSpace(loc.Name) orderby loc.Name select loc.Name).ToArray();
                ModMonitor.Log($"Could not find a location with that name. Must be one of [{string.Join(", ", locationNames)}].", LogLevel.Error);
                return;
            }

            // remove objects
            int removed = 0;
            foreach ((Vector2 tile, var obj) in location.Objects.Pairs.ToArray())
            {
                if (obj.QualifiedItemId == args[1])
                {
                    location.Objects.Remove(tile);
                    removed++;
                }
            }

            ModMonitor.Log($"Command removed {removed} objects at {location.NameOrUniqueName}", LogLevel.Info);
            return;
        }

        private void DebugListActiveLocations(string command, string[] args)
        {
            if (args.Length > 0)
            {
                Monitor.Log($"This command does not take any arguments", LogLevel.Warn);
                return;
            }

            if (!Context.IsWorldReady)
            {
                ModMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            List<string> activeLocations = [];

            foreach (GameLocation location in Game1.locations)
            {
                if (location.isAlwaysActive.Value == true)
                {
                    activeLocations.Add(location.Name);
                }
            }

            // Print out the comma-separated list of active locations
            string activeLocationsStr = string.Join(", ", activeLocations);
            ModMonitor.Log($"Active locations: {activeLocationsStr}", LogLevel.Info);
        }
    }
}