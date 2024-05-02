using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using xTile.ObjectModel;
using xTile.Tiles;
using System;
using System.Linq;

namespace AnythingAnywhere.Framework.Patches.GameLocations
{
    internal class GameLocationPatch : PatchTemplate
    {
        internal GameLocationPatch(Harmony harmony) : base(harmony, typeof(GameLocation)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(GameLocation.CanPlaceThisFurnitureHere), nameof(CanPlaceThisFurnitureHerePostfix), [typeof(Furniture)]);
            Patch(PatchType.Postfix, nameof(GameLocation.isBuildable), nameof(IsBuildablePostfix), [typeof(Vector2), typeof(bool)]);
            Patch(PatchType.Postfix, nameof(GameLocation.IsBuildableLocation), nameof(IsBuildableLocationPostfix));
            Patch(PatchType.Postfix, nameof(GameLocation.doesTileHaveProperty), nameof(DoesTileHavePropertyPostfix), [typeof(int), typeof(int), typeof(string), typeof(string), typeof(bool)]);
            Patch(PatchType.Postfix, nameof(GameLocation.CanFreePlaceFurniture), nameof(CanFreePlaceFurniturePostfix));
            Patch(PatchType.Prefix, nameof(GameLocation.spawnWeedsAndStones), nameof(SpawnWeedsAndStonesPrefix), [typeof(int), typeof(bool), typeof(bool)]);
            Patch(PatchType.Prefix, nameof(GameLocation.loadWeeds), nameof(LoadWeedsPrefix));
        }

        // Sets all furniture types as placeable in all locations.
        private static void CanPlaceThisFurnitureHerePostfix(GameLocation __instance, Furniture furniture, ref bool __result)
        {
            if (ModEntry.modConfig.EnablePlacing)
                    __result = true;
        }

        // Sets tiles buildable for construction (just visual)
        private static void IsBuildablePostfix(GameLocation __instance, Vector2 tileLocation, ref bool __result, bool onlyNeedsToBePassable = false)
        {
            if (ModEntry.modConfig.EnableBuilding)
            {
                if (ModEntry.modConfig.EnableBuildAnywhere)
                {
                    __result = true;
                }
                else if (!__instance.IsOutdoors && !ModEntry.modConfig.EnableBuildingIndoors)
                {
                    __result = false;
                }
                else if (__instance.isTilePassable(tileLocation) && !__instance.isWaterTile((int)tileLocation.X, (int)tileLocation.Y))
                {
                    __result = !__instance.IsTileOccupiedBy(tileLocation, CollisionMask.All, CollisionMask.All);
                }
                else
                {
                    __result = false; // Set to false if the tile is not passable
                }
            }
        }

        // Set all locations buildable.
        private static void IsBuildableLocationPostfix(GameLocation __instance, ref bool __result)
        {
            if (ModEntry.modConfig.EnableBuilding)
            {
                if (ModEntry.modConfig.EnableBuildingIndoors)
                    __result = true;

                if (__instance.IsOutdoors)
                    __result = true;
            }

            if (ModEntry.modConfig.EnableBuildAnywhere)
                __result = true;
        }

        // Set all tiles as diggable
        private static void DoesTileHavePropertyPostfix(GameLocation __instance, int xTile, int yTile, string propertyName, string layerName, ref string __result)
        {
            if (!Context.IsWorldReady || !__instance.farmers.Any() || !(propertyName == "Diggable") || !(layerName == "Back") || !ModEntry.modConfig.EnablePlanting)
            {
                return;
            }

            Tile tile = __instance.Map.GetLayer("Back")?.Tiles[xTile, yTile];
            if (tile?.TileSheet == null)
            {
                return;
            }
            string text = null;
            IPropertyCollection tileIndexProperties = tile.TileIndexProperties;
            if (tileIndexProperties != null && tileIndexProperties.TryGetValue("Type", out var value))
            {
                text = value?.ToString();
            }
            else
            {
                IPropertyCollection properties = tile.Properties;
                if (properties != null && properties.TryGetValue("Type", out value))
                {
                    text = value?.ToString();
                }
            }
            if (ModEntry.modConfig.EnableDiggingAll)
            {
                __result = "T";
            }
            if (text == "Dirt" || text == "Grass")
            {
                __result = "T";
            }
        }

        // Allows longer reach when placing furniture
        private static void CanFreePlaceFurniturePostfix(GameLocation __instance, ref bool __result)
        {
            if (ModEntry.modConfig.EnablePlacing)
                __result = true;
        }

        private static bool SpawnWeedsAndStonesPrefix(GameLocation __instance, int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
        {
            if (!ModEntry.modConfig.EnableGoldClockAnywhere)
                return true;

            bool hasGoldClock = __instance.buildings.Any(building => building.buildingType.Value == "Gold Clock");
            if (hasGoldClock && !Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
                return false;

            return true;
        }

        private static bool LoadWeedsPrefix(GameLocation __instance)
        {
            if (!ModEntry.modConfig.EnableGoldClockAnywhere)
                return true;

            bool hasGoldClock = __instance.buildings.Any(building => building.buildingType.Value == "Gold Clock");
            if (hasGoldClock && !Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
                return false;

            return true;
        }
    }
}