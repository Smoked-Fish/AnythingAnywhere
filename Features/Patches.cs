using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Data;
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

        /// <summary>The beginning of each each map/tile property name implemented by this mod.</summary>
        private static readonly string PropertyPrefix = "Espy.AnythingAnywhere/";

        /// <summary>The name of the map property used by this patch.</summary>
        private static string MapPropertyName { get; set; } = null;

        /// <summary>True if this patch is currently applied.</summary>
        private static bool Applied { get; set; } = false;

        internal static void Initialise(Harmony harmony, IMonitor monitor, Func<ModConfig> config, IReflectionHelper reflectionHelper)
        {
            s_harmony = harmony;
            s_monitor = monitor;
            s_config = config;
            s_reflectionHelper = reflectionHelper;

            //initialize assets/properties
            MapPropertyName = PropertyPrefix + "AllowFurniture"; //assign map property name

            s_monitor.Log($"Applying Harmony patch \"{nameof(Patches)}\": postfixing method \"GameLocation.CanPlaceThisFurnitureHere(Furniture)\".", LogLevel.Trace);

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

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.GetAdditionalFurniturePlacementStatus), [typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(GetAdditionalFurniturePlacementStatus_postfix))
            );

            s_harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction), [typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(placementAction_prefix))
            );

            Applied = true;
        }


        /// <summary>Determines whether this mod's settings allow furniture beds to be placed at the given location.</summary>
        /// <param name="location">The location of the bed.</param>
        /// <returns>True if beds should be placeable here; false if they should not.</returns>
        private static bool ShouldFurnitureBePlaceableHere(GameLocation location)
        {
            ModConfig config = s_config();

            if (location == null)
                return false;

            if (config.AllowAllGroundFurniture) //if config allows placement
            {
                if (s_monitor?.IsVerbose == false)
                    s_monitor.LogOnce($"Allowing furniture placement due to config.json settings.", LogLevel.Trace);
                return true;
            }

            if (location.Map.Properties.TryGetValue(MapPropertyName, out var mapPropertyObject)) //if the location has a non-null map property
            {
                string mapProperty = mapPropertyObject?.ToString() ?? ""; //get the map property as a string

                bool result = !mapProperty.Trim().StartsWith("F", StringComparison.OrdinalIgnoreCase); //true if the property's value is NOT "false"

                if (s_monitor?.IsVerbose == false)
                {
                    if (result)
                        s_monitor.Log($"Allowing furniture placement. Location: {location?.Name}. Map property value: \"{mapProperty}\".", LogLevel.Trace);
                    else
                        s_monitor.Log($"NOT allowing furniture placement. Location: {location?.Name}. Map property value: \"{mapProperty}\".", LogLevel.Trace);
                }

                return result;
            }

            if (s_monitor?.IsVerbose == false)
                s_monitor.Log($"NOT allowing furniture placement; no relevant map or tile property. Location: {location?.Name}.", LogLevel.Trace);

            return false; //default to null
        }

        /// <summary>Allows placement of furniture based on custom map properties.</summary>
        /// <param name="__instance">The instance calling the original method.</param>'
        /// <param name="furniture">The furniture being checked.</param>
        /// <param name="__result">The result of the original method. True if the furniture can be placed; false otherwise.</param>
        private static void CanPlaceThisFurnitureHere_postfix(GameLocation __instance, Furniture furniture, ref bool __result)
        {
            try
            {
                if (ShouldFurnitureBePlaceableHere(__instance))
                    __result = true; //allow it
/*                if (furniture.furniture_type.Value == 15 || furniture is BedFurniture) //if the furniture is a bed
                    if (furniture is TV || furniture is FishTankFurniture || furniture is BedFurniture || furniture is StorageFurniture || furniture is RandomizedPlantFurniture)
                    {

                    }*/
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"{nameof(Patches)}\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        /// <summary>Get the reason the furniture can't be placed at a given position, if applicable.</summary>
        /// <returns>
        ///   <list type="bullet">
        ///     <item><description>0: valid placement.</description></item>
        ///     <item><description>1: the object is a wall placed object but isn't being placed on a wall.</description></item>
        ///     <item><description>2: the object can't be placed here due to the tile being marked as not furnishable.</description></item>
        ///     <item><description>3: the object isn't a wall placed object, but is trying to be placed on a wall.</description></item>
        ///     <item><description>4: the current location isn't decorable.</description></item>
        ///     <item><description>-1: general fail condition.</description></item>
        ///   </list>
        /// </returns>
        private static void GetAdditionalFurniturePlacementStatus_postfix(Furniture __instance, GameLocation location, int x, int y, Farmer who, ref int __result)
        {
            try
            {
                ModConfig config = s_config();

                if (config.AllowAllWallFurniture)
                {
                    __result = 0;
                }
                return;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"{nameof(Patches)}\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }

        }

        private static bool placementAction_prefix(StardewValley.Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
        {
            try
            {
                Vector2 placementTile = new Vector2(x / 64, y / 64);

                if (__instance.QualifiedItemId == "(BC)216") //if this object is a Mini-Fridge

                {
                    if (!location.objects.ContainsKey(placementTile) && ShouldFurnitureBePlaceableHere(location)) //if this tile is unobstructed (original check) AND this patch should allow placement 
                    {
                        //apply changes normally made at the start of the original method
                        __instance.setHealth(10);
                        if (who != null)
                            __instance.owner.Value = who.UniqueMultiplayerID;
                        else
                            __instance.owner.Value = Game1.player.UniqueMultiplayerID;

                        //imitate the original method's code for successful placement
                        Chest fridge = new Chest("216", placementTile, 217, 2)
                        {
                            shakeTimer = 50
                        };
                        fridge.fridge.Value = true;
                        location.objects.Add(placementTile, fridge);
                        location.playSound("hammer");

                        __result = true; //return true
                        return false; //skip the original method
                    }
                }

                return true; //default result: run the original method
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"{nameof(Patches)}..{nameof(placementAction_prefix)}\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }

    }
}