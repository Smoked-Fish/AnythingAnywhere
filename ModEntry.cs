using HarmonyLib;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Locations;
using StardewValley.TokenizableStrings;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using AnythingAnywhere.Framework.UI;
using AnythingAnywhere.Framework.Interfaces;
using AnythingAnywhere.Framework.Patches.Menus;
using AnythingAnywhere.Framework.Patches.Locations;
using AnythingAnywhere.Framework.Patches.GameLocations;
using AnythingAnywhere.Framework.Patches.StandardObjects;
using AnythingAnywhere.Framework.Patches.TerrainFeatures;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using Common.Managers;
using Common.Util;

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

        private static Harmony harmony;
        private static bool buildingConfigChanged = false;

        public override void Entry(IModHelper helper)
        {
            // Setup the monitor, helper, config and multiplayer
            ModMonitor = Monitor;
            ModHelper = helper;
            Config = Helper.ReadConfig<ModConfig>();
            Multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup the manager
            ApiManager = new ApiManager(helper, ModMonitor);

            // Load the Harmony patches
            harmony = new Harmony(this.ModManifest.UniqueID);

            // Apply GameLocation patches
            new GameLocationPatch(harmony).Apply();

            // Apply Location patches
            new FarmHousePatch(harmony).Apply();

            // Apply Menu patches
            new CarpenterMenuPatch(harmony).Apply();
            new AnimalQueryMenuPatch(harmony).Apply();

            // Apply StandardObject patches
            new CaskPatch(harmony).Apply();
            new FurniturePatch(harmony).Apply();
            new MiniJukeboxPatch(harmony).Apply();
            new ObjectPatch(harmony).Apply();

            // Apply TerrainFeature patches
            new FruitTreePatch(harmony).Apply();
            new TreePatch(harmony).Apply();


            // Add debug commands
            helper.ConsoleCommands.Add("aa_remove_objects", "Removes all objects of a specified ID at a specified location.\n\nUsage: aa_remove_objects [LOCATION] [OBJECT_ID]", this.DebugRemoveObjects);
            helper.ConsoleCommands.Add("aa_remove_furniture", "Removes all furniture of a specified ID at a specified location.\n\nUsage: aa_remove_furniture [LOCATION] [FURNITURE_ID]", this.DebugRemoveFurniture);

            // Hook into Game events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;

            // Hook into Custom events
            ButtonOptions.Click += this.OnClick;
            Config.ConfigChanged += OnConfigChanged;
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
                ConfigManager.AddOption(nameof(ModConfig.BuildMenu));
                ConfigManager.AddOption(nameof(ModConfig.WizardBuildMenu));
                ConfigManager.AddOption(nameof(ModConfig.BuildModifier));
                ConfigManager.AddOption(nameof(ModConfig.EnableGreenhouse));
                ConfigManager.AddOption(nameof(ModConfig.RemoveBuildConditions));
                ConfigManager.AddOption(nameof(ModConfig.EnableBuildingIndoors));
                ConfigManager.AddOption(nameof(ModConfig.BypassMagicInk));
                ConfigManager.AddHorizontalSeparator();
                ConfigManager.AddButtonOption("BlacklistedLocations", "BlacklistedLocations", "BlacklistCurrentLocation");


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
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                if (Config.BlacklistedLocations != null && Config.BlacklistedLocations.Contains(location.NameOrUniqueName)) continue;
                if (location.buildings.Any() && location.isAlwaysActive.Value == false)
                {
                    location.Map.Properties.TryGetValue("CanBuildHere", out var value);
                    if (value == null)
                    {
                        location.Map.Properties.Add("CanBuildHere", "T");
                    }
                    else
                    {
                        value = "T";
                    }
                    location.isAlwaysActive.Value = true;
                }
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !Config.EnableBuilding)
                return;

            if (Config.BuildMenu.JustPressed() && Config.EnableBuilding)
                HandleBuildButtonPress("Robin");

            if (Config.WizardBuildMenu.JustPressed() && Config.EnableBuilding)
                HandleBuildButtonPress("Wizard");
        }

        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                if (location.buildings.Any() && location.isAlwaysActive.Value == true)
                {
                    ModMonitor.Log($"Location Name: {location.DisplayName}, Active: {location.isAlwaysActive.Value}", LogLevel.Debug);
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.Name.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit(
                    asset =>
                    {
                        var data = asset.AsDictionary<string, BuildingData>().Data;
                        foreach (var buildingDataKey in data.Keys.ToList()) 
                        {
                            data[buildingDataKey] = ModifyBuildingData(data[buildingDataKey], Config.EnableInstantBuild, Config.EnableGreenhouse, Config.RemoveBuildConditions);
                        }
                    }, AssetEditPriority.Late);
                return;
            }

            if (e.Name.IsEquivalentTo("Data/Locations"))
            {
                e.Edit(
                    asset =>
                    {
                        var data = asset.AsDictionary<string, LocationData>().Data;
                        foreach (var LocationDataKey in data.Keys.ToList())
                        {
                            data[LocationDataKey] = ModifyLocationData(data[LocationDataKey], Config.EnablePlanting);
                        }
                    }, AssetEditPriority.Late);
                return;
            }
        }

        private void OnConfigChanged(object sender, ConfigChangedEventArgs e)
        {
            if (Equals(e.OldValue, e.NewValue)) return;

            if (e.ConfigName == nameof(ModConfig.EnablePlanting))
            {
                ModHelper.GameContent.InvalidateCache("Data/Locations");
            }

            if (e.ConfigName == nameof(ModConfig.RemoveBuildConditions) || e.ConfigName == nameof(ModConfig.EnableInstantBuild) || e.ConfigName == nameof(ModConfig.EnableGreenhouse))
            {
                buildingConfigChanged = true; // Doesn't work if I don't do this
            }
        }

        private void OnClick(ButtonClickEventArgs e)
        {
            if (e.FieldID.Equals("BlacklistCurrentLocation"))
            {
                if (!Context.IsWorldReady)
                {
                    Game1.playSound("thudStep");
                    return;
                }
                else if (Game1.player.currentLocation.IsFarm)
                {
                    Game1.playSound("thudStep");
                    return;
                }
                else if (Config.BlacklistedLocations.Contains(Game1.player.currentLocation.NameOrUniqueName))
                {
                    Game1.playSound("thudStep");
                    return;
                }
                else
                {
                    Game1.playSound("backpackIN");
                    Config.BlacklistedLocations.Add(Game1.player.currentLocation.NameOrUniqueName);
                    if (Game1.player.currentLocation.IsBuildableLocation())
                    {
                        Game1.currentLocation.Map.Properties.Remove("CanBuildHere");
                    }
                }
            }
            else
            {
                Game1.playSound("backpackIN");
                Config.InitializeDefaultConfig(e.FieldID);
                PageHelper.OpenPage(PageHelper.CurrPage);
            }
        }

        private static BuildingData ModifyBuildingData(BuildingData data, bool enableInstantBuild, bool enableGreenhouse, bool removeBuildConditions)
        {
            if (enableGreenhouse && IsGreenhouse(data))
                SetGreenhouseAttributes(data);

            if (enableInstantBuild)
                SetInstantBuildAttributes(data);

            if (removeBuildConditions)
                RemoveBuildConditions(data);

            return data;
        }

        private static bool IsGreenhouse(BuildingData data)
        {
            return TokenParser.ParseText(data.Name) == Game1.content.LoadString("Strings\\Buildings:Greenhouse_Name");
        }

        private static void SetGreenhouseAttributes(BuildingData data)
        {
            // Define greenhouse materials
            List<BuildingMaterial> greenhouseMaterials = new List<BuildingMaterial>
            {
                new BuildingMaterial { ItemId = "(O)709", Amount = 100 },
                new BuildingMaterial { ItemId = "(O)338", Amount = 20 },
                new BuildingMaterial { ItemId = "(O)337", Amount = 10 },
            };

            // Set greenhouse attributes
            data.Builder = Game1.builder_robin;
            data.BuildCost = 150000;
            data.BuildDays = 3;
            data.BuildMaterials = greenhouseMaterials;
            data.BuildCondition = "PLAYER_HAS_MAIL Host ccPantry";
        }

        private static void SetInstantBuildAttributes(BuildingData data)
        {
            data.MagicalConstruction = true;
            data.BuildCost = 0;
            data.BuildDays = 0;
            data.BuildMaterials = [];
        }

        private static void RemoveBuildConditions(BuildingData data)
        {
            data.BuildCondition = "";
        }

        private static LocationData ModifyLocationData(LocationData data, bool enablePlanting)
        {
            if (enablePlanting)
            {
                data.CanPlantHere = true;
            }

            return data;
        }

        private void HandleBuildButtonPress(string builder)
        {
            if (Context.IsPlayerFree && Game1.activeClickableMenu == null)
            {
                if (buildingConfigChanged)
                {
                    ModHelper.GameContent.InvalidateCache("Data/Buildings");
                    buildingConfigChanged = false;
                }
                ActivateBuildAnywhereMenu(builder);
            }
            else if (Game1.activeClickableMenu is BuildAnywhereMenu)
            {
                Game1.displayFarmer = true;
                ((BuildAnywhereMenu)Game1.activeClickableMenu).returnToCarpentryMenu();
                ((BuildAnywhereMenu)Game1.activeClickableMenu).exitThisMenu();
            }
        }

        private void ActivateBuildAnywhereMenu(string builder)
        {
            if (!Game1.currentLocation.IsOutdoors && !Config.EnableBuildingIndoors)
            {
                Game1.addHUDMessage(new HUDMessage(TranslationHelper.GetByKey("Message.AnythingAnywhere.NoBuildingIndoors"), HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                return;
            }
            bool magicInkCheck = !(Game1.getFarmer(Game1.player.UniqueMultiplayerID).hasMagicInk || Config.BypassMagicInk);
            if (builder == "Wizard" && magicInkCheck && !Config.EnableInstantBuild)
            {
                Game1.addHUDMessage(new HUDMessage(TranslationHelper.GetByKey("Message.AnythingAnywhere.NoMagicInk"), HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                return;
            }

            if (!Game1.currentLocation.IsBuildableLocation())
            {
                Game1.currentLocation.Map.Properties.Add("CanBuildHere", "T");
                Game1.currentLocation.isAlwaysActive.Value = true;
            }

            Game1.activeClickableMenu = new BuildAnywhereMenu(builder, Game1.player.currentLocation);
        }

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
    }
}