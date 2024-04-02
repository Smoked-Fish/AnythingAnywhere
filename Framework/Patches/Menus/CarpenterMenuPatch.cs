using AnythingAnywhere.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Object = StardewValley.Object;


namespace AnythingAnywhere.Framework.Patches.Menus
{
    internal class CarpenterMenuPatch : PatchTemplate
    {
        private readonly Type _object = typeof(CarpenterMenu);

        internal CarpenterMenuPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(CarpenterMenu.tryToBuild)), prefix: new HarmonyMethod(GetType(), nameof(TryToBuildPrefix)));
            //harmony.Patch(AccessTools.Method(_object, nameof(CarpenterMenu.performHoverAction), new[] { typeof(int), typeof(int) }), postfix: new HarmonyMethod(GetType(), nameof(PerformHoverActionPostfix)));

        }

        // Build in walls
        private static bool TryToBuildPrefix(CarpenterMenu __instance, ref bool __result)
        {
            if (Game1.activeClickableMenu is BuildAnywhereMenu && ModEntry.modConfig.EnableFreeBuild)
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

        private static void PerformHoverActionPostfix(CarpenterMenu __instance, int x, int y)
        {
            if (__instance.onFarm && ModEntry.modConfig.EnableFreeBuild)
            {
                if ((!__instance.upgrading && !__instance.demolishing && !__instance.moving && !__instance.painting) || __instance.freeze)
                {
                    return;
                }
                foreach (Building building in __instance.TargetLocation.buildings)
                {
                    building.color = Color.White;
                }
                Vector2 tile = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
                Building b = __instance.TargetLocation.getBuildingAt(tile) ?? __instance.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 1f)) ?? __instance.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 2f)) ?? __instance.TargetLocation.getBuildingAt(new Vector2(tile.X, tile.Y + 3f));
                BuildingData data = b?.GetData();
                if (data != null)
                {
                    int stickOutTilesHigh = (data.SourceRect.IsEmpty ? b.texture.Value.Height : b.GetData().SourceRect.Height) * 4 / 64 - (int)b.tilesHigh.Value;
                    if ((float)((int)b.tileY.Value - stickOutTilesHigh) > tile.Y)
                    {
                        b = null;
                    }
                }
                if (__instance.upgrading)
                {
                    if (b != null)
                    {
                        b.color = ((b.buildingType.Value == __instance.Blueprint.UpgradeFrom) ? (Color.Lime * 0.8f) : (Color.Red * 0.8f));
                    }
                }
                else if (__instance.demolishing)
                {
                    if (b != null && __instance.hasPermissionsToDemolish(b) && __instance.CanDemolishThis(b))
                    {
                        b.color = Color.Red * 0.8f;
                    }
                }
                else if (__instance.moving)
                {
                    if (b != null && __instance.hasPermissionsToMove(b))
                    {
                        b.color = Color.Lime * 0.8f;
                    }
                }
                else if (__instance.painting && b != null && (b.CanBePainted() || b.CanBeReskinned(ignoreSeparateConstructionEntries: true)) && __instance.HasPermissionsToPaint(b))
                {
                    b.color = Color.Lime * 0.8f;
                }
            }
        }
    }
}
