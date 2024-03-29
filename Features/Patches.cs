using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace AnythingAnywhere.Features
{
    internal class Patches
    {
        private static Harmony s_harmony;
        private static IMonitor s_monitor;
        private static Func<ModConfig> s_config;
        private static IReflectionHelper s_reflectionHelper;

        /// <summary>True if this patch is currently applied.</summary>
        private static bool Applied { get; set; } = false;

        internal static void Initialise(Harmony harmony, IMonitor monitor, Func<ModConfig> config, IReflectionHelper reflectionHelper)
        {
            s_harmony = harmony;
            s_monitor = monitor;
            s_config = config;
            s_reflectionHelper = reflectionHelper;

            ApplyPatches();
        }

        private static void ApplyPatches()
        {
            if (Applied)
                return;

            s_harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.CanPlaceThisFurnitureHere), [typeof(Furniture)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(CanPlaceThisFurnitureHere_postfix))
            );
            s_monitor.Log("Applying Harmony patch: postfixing method \"GameLocation.CanPlaceThisFurnitureHere\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.GetAdditionalFurniturePlacementStatus), [typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(GetAdditionalFurniturePlacementStatus_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Furniture.GetAdditionalFurniturePlacementStatus\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction), [typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(placementAction_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Object.placementAction\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(MiniJukebox), nameof(MiniJukebox.checkForAction), [typeof(Farmer), typeof(bool)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(checkForAction_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"MiniJukebox.checkForAction\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isBuildable), [typeof(Vector2), typeof(bool)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(isBuildable_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"GameLocation.isBuildable\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.IsBuildableLocation)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(IsBuildableLocation_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"GameLocation.IsBuildableLocation_postfix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(CarpenterMenu), nameof(CarpenterMenu.tryToBuild)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(tryToBuild_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"CarpenterMenu.tryToBuild_prefix\".", LogLevel.Trace);


            Applied = true;
        }

        private static void CanPlaceThisFurnitureHere_postfix(GameLocation __instance, Furniture furniture, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();

                bool allWallFurniture =
                    ((int)furniture.furniture_type.Value == 6 ||
                    (int)furniture.furniture_type.Value == 17 ||
                    (int)furniture.furniture_type.Value == 13 ||
                    furniture.QualifiedItemId == "(F)1293");

                if (config.AllowAllWallFurniture && allWallFurniture)
                    __result = true;

                if (config.AllowAllGroundFurniture)
                    __result = true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"CanPlaceThisFurnitureHere_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        private static void GetAdditionalFurniturePlacementStatus_postfix(Furniture __instance, GameLocation location, int x, int y, Farmer who, ref int __result)
        {
            try
            {
                ModConfig config = s_config();

                bool allWallFurniture =
                    ((int)__instance.furniture_type.Value == 6 ||
                    (int)__instance.furniture_type.Value == 17 ||
                    (int)__instance.furniture_type.Value == 13 ||
                    __instance.QualifiedItemId == "(F)1293");

                if (location.CanPlaceThisFurnitureHere(__instance))
                {
                    if (allWallFurniture && config.AllowAllWallFurniture && !(location is IslandFarmHouse || location is FarmHouse))
                        __result = 0;

                    if (allWallFurniture && config.AllowAllWallFurnitureFarmHouse)
                        __result = 0;

                    if (__instance is BedFurniture)
                        __result = 0;
                }
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"GetAdditionalFurniturePlacementStatus_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        private static bool placementAction_prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();
                Vector2 placementTile = new Vector2(x / 64, y / 64);
                __instance.setHealth(10);
                __instance.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

                switch (__instance.QualifiedItemId)
                {
                    case "(BC)216": // Mini-Fridge
                        Chest fridge = new Chest("216", placementTile, 217, 2)
                        {
                            shakeTimer = 50
                        };
                        fridge.fridge.Value = true;
                        location.objects.Add(placementTile, fridge);
                        location.playSound("hammer");

                        __result = true;
                        return false; //skip original method
                    case "(BC)238": // Mini-Obelisk
                        if (!config.AllowMiniObelisksAnywhere)
                        {
                            __result = false;
                            return true;
                        }
                        Vector2 obelisk1 = Vector2.Zero;
                        Vector2 obelisk2 = Vector2.Zero;

                        foreach (KeyValuePair<Vector2, StardewValley.Object> o2 in location.objects.Pairs)
                        {
                            if (o2.Value.QualifiedItemId == "(BC)238")
                            {
                                if (obelisk1.Equals(Vector2.Zero))
                                {
                                    obelisk1 = o2.Key;
                                }
                                else if (obelisk2.Equals(Vector2.Zero))
                                {
                                    obelisk2 = o2.Key;
                                    break;
                                }
                            }
                        }

                        // Find existing obelisks
                        /*                        var obeliskLocations = Game1.locations
                                                    .SelectMany(l => l.objects.Pairs)
                                                    .Where(pair => pair.Value.QualifiedItemId == "(BC)238")
                                                    .Select(pair => pair.Key)
                                                    .ToList();*/

                        // Assign obelisk locations
                        /*                        if (obeliskLocations.Count >= 2)
                                                {
                                                    obelisk1 = obeliskLocations[0];
                                                    obelisk2 = obeliskLocations[1];
                                                }*/

                        if (!obelisk1.Equals(Vector2.Zero) && !obelisk2.Equals(Vector2.Zero))
                        {
                            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceTwo"));
                            __result = false;
                            return false; //skip original method
                        }
                        __result = true;
                        break;
                    case "(BC)254": // Ostrich Incubator
                        __result = true;
                        break;
                        
                }
                if (__result)
                {
                    StardewValley.Object toPlace = (StardewValley.Object)__instance.getOne();
                    toPlace.shakeTimer = 50;
                    toPlace.Location = location;
                    toPlace.TileLocation = placementTile;
                    toPlace.performDropDownAction(who);

                    location.objects.Add(placementTile, toPlace);
                    toPlace.initializeLightSource(placementTile);
                    location.playSound("woodyStep");
                    return false; //skip original method
                }

                return true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"placementAction_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }

        private static bool checkForAction_prefix(MiniJukebox __instance, Farmer who, ref bool __result, bool justCheckingForActivity = false)
        {
            try
            {
                ModConfig config = s_config();

                if (!config.EnableJukeboxFunctionality) 
                {
                    __result = false;
                    return true; //run the original method
                }
                if (justCheckingForActivity)
                {
                    __result = true;
                    return false;
                }
                GameLocation location = __instance.Location;
                if (location.IsOutdoors && location.IsRainingHere())
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_OutdoorRainy"));
                }
                else
                {
                    List<string> jukeboxTracks = Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation);
                    jukeboxTracks.Insert(0, "turn_off");
                    jukeboxTracks.Add("random");
                    Game1.activeClickableMenu = new ChooseFromListMenu(jukeboxTracks, __instance.OnSongChosen, isJukebox: true, location.miniJukeboxTrack.Value);
                }
                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"checkForAction_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }

        private static void isBuildable_postfix(GameLocation __instance, Vector2 tileLocation, ref bool __result, bool onlyNeedsToBePassable = false)
        {
            try
            {
                ModConfig config = s_config();
                if (config.EnableBuilding)
                {
                    if (__instance.isTilePassable(tileLocation) && !__instance.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
                    {
                        __result = !__instance.IsTileOccupiedBy(tileLocation, CollisionMask.All, CollisionMask.All);
                    }
                    else if (config.EnableFreeBuild)
                    {
                        __result = true;
                    }
                    else
                    {
                        __result = false; // Set to false if the tile is not passable
                    }
                }
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"isBuildable_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        private static void IsBuildableLocation_postfix(GameLocation __instance, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();

                if (config.EnableBuilding)
                    __result = true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"IsBuildableLocation_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        private static bool tryToBuild_prefix(CarpenterMenu __instance, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();

                if (Game1.activeClickableMenu is BuildAnywhereMenu && config.EnableFreeBuild)
                {
                    NetString skinId = __instance.currentBuilding.skinId;
                    Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
                    if (__instance.TargetLocation.buildStructure(__instance.currentBuilding.buildingType.Value, tileLocation, Game1.player, out var building, /*__instance.Blueprint.MagicalConstruction*/ true, true))
                    {
                        building.skinId.Value = skinId.Value;
                        if (building.isUnderConstruction())
                        {
                            Game1.netWorldState.Value.MarkUnderConstruction(__instance.Builder, building);
                        }
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"tryToBuild_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }
    }
}