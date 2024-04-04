using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Menus;
using AnythingAnywhere.Framework.UI;
using Microsoft.Xna.Framework;
using Netcode;
using System;


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
        }

        // Build in walls
        private static bool TryToBuildPrefix(CarpenterMenu __instance, ref bool __result)
        {
            if (Game1.activeClickableMenu is BuildAnywhereMenu && ModEntry.modConfig.EnableBuildAnywhere)
            {
                NetString skinId = __instance.currentBuilding.skinId;
                Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
                if (__instance.TargetLocation.buildStructure(__instance.currentBuilding.buildingType.Value, tileLocation, Game1.player, out var building, __instance.Blueprint.MagicalConstruction, true))
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
            return true;
        }
    }
}
