using Common.Helpers;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.ObjectModel;
using xTile.Tiles;

namespace AnythingAnywhere.Framework.Patches;
internal sealed class FarmingPatches : PatchHelper
{
    public void Apply()
    {
        Patch<GameLocation>(PatchType.Postfix, nameof(GameLocation.doesTileHaveProperty), nameof(DoesTileHavePropertyPostfix), [typeof(int), typeof(int), typeof(string), typeof(string), typeof(bool)]);
        Patch<GameLocation>(PatchType.Prefix, nameof(GameLocation.CheckItemPlantRules), nameof(CheckItemPlantRulesPrefix), [typeof(string), typeof(bool), typeof(bool), typeof(string).MakeByRefType()]);
        Patch<GameLocation>(PatchType.Postfix, nameof(GameLocation.SeedsIgnoreSeasonsHere), nameof(SeedsIgnoreSeasonsHerePostfix));

        Patch<IslandWest>(PatchType.Prefix, nameof(IslandWest.CanPlantSeedsHere), nameof(CanPlantSeedsHerePrefix), [typeof(string), typeof(int), typeof(int), typeof(bool), typeof(string).MakeByRefType()]);
        Patch<IslandWest>(PatchType.Prefix, nameof(IslandWest.CanPlantTreesHere), nameof(CanPlantTreesHerePrefix), [typeof(string), typeof(int), typeof(int), typeof(string).MakeByRefType()]);
        Patch<Town>(PatchType.Prefix, nameof(Town.CanPlantTreesHere), nameof(CanPlantTreesHerePrefix), [typeof(string), typeof(int), typeof(int), typeof(string).MakeByRefType()]);

        Patch<FruitTree>(PatchType.Postfix, nameof(FruitTree.IsGrowthBlocked), nameof(IsGrowthBlockedPostfix), [typeof(Vector2), typeof(GameLocation)]);
        Patch<FruitTree>(PatchType.Postfix, nameof(FruitTree.IsTooCloseToAnotherTree), nameof(IsTooCloseToAnotherTreePostfix), [typeof(Vector2), typeof(GameLocation), typeof(bool)]);
        Patch<Tree>(PatchType.Postfix, nameof(Tree.IsGrowthBlockedByNearbyTree), nameof(IsGrowthBlockedByNearbyTreePostfix));

        Patch<FruitTree>(PatchType.Postfix, nameof(FruitTree.IgnoresSeasonsHere), nameof(IgnoreSeasonsHerePostfix));
        Patch<FruitTree>(PatchType.Postfix, nameof(FruitTree.IsInSeasonHere), nameof(IsInSeasonHerePostfix));

        Patch<JunimoHut>(PatchType.Postfix, nameof(JunimoHut.dayUpdate), nameof(DayUpdatePostfix), [typeof(int)]);
    }

    // Set all tiles as diggable
    private static void DoesTileHavePropertyPostfix(GameLocation __instance, int xTile, int yTile, string propertyName, string layerName, ref string __result)
    {
        if (!Context.IsWorldReady || !__instance.farmers.Any() || propertyName != "Diggable" || layerName != "Back" || !ModEntry.Config.EnablePlanting)
            return;

        Tile? tile = __instance.Map.GetLayer("Back")?.Tiles[xTile, yTile];
        if (tile?.TileSheet == null)
        {
            return;
        }
        string? text = null;
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
        if (ModEntry.Config.EnableDiggingAll)
        {
            __result = "T";
        }
        if (text is "Dirt" or "Grass")
        {
            __result = "T";
        }
    }

    // Enables tree and seed planting everywhere
    private static bool CheckItemPlantRulesPrefix(GameLocation __instance, string itemId, bool isGardenPot, bool defaultAllowed, ref string deniedMessage, ref bool __result)
    {
        if (!ModEntry.Config.EnablePlanting)
            return true;

        __result = true;
        return false;
    }

    // Enable planing in all seasons
    private static void SeedsIgnoreSeasonsHerePostfix(GameLocation __instance, ref bool __result)
    {
        if (ModEntry.Config.DisableSeasonRestrictions)
            __result = true;
    }

    private static bool CanPlantSeedsHerePrefix(IslandWest __instance, string itemId, int tileX, int tileY, bool isGardenPot, ref string deniedMessage, ref bool __result)
    {
        if (!ModEntry.Config.EnablePlanting)
            return true;

        __result = true;
        return false;
    }

    private static bool CanPlantTreesHerePrefix(IslandWest __instance, string itemId, int tileX, int tileY, ref string deniedMessage, ref bool __result)
    {
        if (!ModEntry.Config.EnablePlanting)
            return true;

        __result = true;
        return false;
    }

    public static void IsGrowthBlockedPostfix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result)
    {
        if (ModEntry.Config.EnableFruitTreeTweaks)
            __result = false;
    }

    private static void IsTooCloseToAnotherTreePostfix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result, bool fruitTreesOnly = false)
    {
        if (ModEntry.Config.EnableFruitTreeTweaks)
            __result = false;
    }

    private static void IsGrowthBlockedByNearbyTreePostfix(Tree __instance, ref bool __result)
    {
        if (ModEntry.Config.EnableWildTreeTweaks)
            __result = false;
    }

    // Fix to use the normal season index but still produce fruit
    private static void IgnoreSeasonsHerePostfix(FruitTree __instance, ref bool __result)
    {
        if (!ModEntry.Config.ForceCorrectTreeSprite || __instance.Location.IsGreenhouse)
            return;

        __result = false;
    }

    // Fix to use the normal season index but still produce fruit
    private static void IsInSeasonHerePostfix(FruitTree __instance, ref bool __result)
    {
        if (!ModEntry.Config.ForceCorrectTreeSprite || __instance.Location.IsGreenhouse)
            return;

        if (ModEntry.Config.DisableSeasonRestrictions)
            __result = true;
    }

    // Send juminos out even if the farmer isn't there.
    private static void DayUpdatePostfix(JunimoHut __instance)
    {
        __instance.shouldSendOutJunimos.Value = true;
    }
}