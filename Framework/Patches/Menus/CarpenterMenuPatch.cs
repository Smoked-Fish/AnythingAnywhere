using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Menus;
using AnythingAnywhere.Framework.UI;
using Microsoft.Xna.Framework;
using Netcode;
using System;
using StardewValley.Buildings;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;
using StardewValley.GameData.Buildings;
using Microsoft.CodeAnalysis;
using StardewValley.Objects;


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
            harmony.Patch(AccessTools.Method(_object, nameof(CarpenterMenu.receiveLeftClick), new[] { typeof(int), typeof(int), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(ReceiveLeftClickPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(CarpenterMenu.IsValidBuildingForLocation), new[] { typeof(string), typeof(BuildingData), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(IsValidBuildingForLocationPrefix)));
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

        // Only show building relocate menu if relocation key is held down
        private static bool ReceiveLeftClickPrefix(CarpenterMenu __instance, int x, int y, bool playSound = true)
        {
            if (!ModEntry.modConfig.EnableBuildingRelocate)
                return true;

            if (Game1.IsMultiplayer)
            {
                if (ModEntry.modConfig.RelocationKey.IsDown())
                {
                    Game1.addHUDMessage(new HUDMessage(I18n.Message_AnythingAnywhere_MultiplayerRelocate(), HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                }
                return true;
            }

            if (Game1.activeClickableMenu is not BuildAnywhereMenu){
                return true;
            }

            if (__instance.freeze)
            {
                return true;
            }

            if (!__instance.onFarm)
            {
                return true;
            }

            if (__instance.cancelButton.containsPoint(x, y))
            {
                if (__instance.onFarm)
                {
                    if (__instance.moving && __instance.buildingToMove != null)
                    {
                        Game1.playSound("cancel");
                        return false;
                    }
                    __instance.returnToCarpentryMenu();
                    Game1.playSound("smallSelect");
                    return false;
                }
                __instance.exitThisMenu();
                Game1.player.forceCanMove();
                Game1.playSound("bigDeSelect");
            }

            if (!__instance.onFarm && __instance.backButton.containsPoint(x, y))
            {
                return true;
            }

            if (!__instance.onFarm && __instance.forwardButton.containsPoint(x, y))
            {
                return true;
            }

            if (!__instance.onFarm || __instance.freeze || Game1.IsFading())
            {
                return true;
            }

            if (__instance.demolishing)
            {
                return true;
            }

            if (__instance.upgrading)
            {
                return true;
            }

            if (__instance.painting)
            {
                return true;
            }

            if (__instance.moving)
            {
                if (__instance.buildingToMove == null)
                {
                    __instance.buildingToMove = __instance.TargetLocation.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64));

                    if (__instance.buildingToMove != null)
                    {

                        if ((int)__instance.buildingToMove.daysOfConstructionLeft.Value > 0)
                        {
                            __instance.buildingToMove = null;
                            return false;
                        }
                        if (!__instance.hasPermissionsToMove(__instance.buildingToMove))
                        {
                            __instance.buildingToMove = null;
                            return false;
                        }

                        if (!ModEntry.modConfig.RelocationKey.IsDown())
                        {
                            __instance.buildingToMove.isMoving = true;
                            Game1.playSound("axchop");
                            return false;
                        }

                        if ((__instance.buildingToMove.GetIndoors() is FarmHouse || __instance.buildingToMove.GetIndoors() is Cabin) && !ModEntry.modConfig.EnableCabinsAnywhere)
                        {
                            Game1.addHUDMessage(new HUDMessage(I18n.Message_AnythingAnywhere_NoMovingBuildingLocation(buildingName: __instance.buildingToMove.GetIndoorsName()), 3));
                            Game1.playSound("cancel");
                            __instance.buildingToMove = null;
                            return false;
                        }

                        if(Game1.getLocationFromName(__instance.BuilderLocationName) != __instance.TargetLocation)
                        {
                            Game1.playSound("cancel");
                            return false;
                        }


                        Game1.playSound("smallSelect");
                        List<KeyValuePair<string, string>> buildableLocations = new List<KeyValuePair<string, string>>();
                        foreach (GameLocation location in Game1.locations)
                        {
                            if (location.IsBuildableLocation())
                            {
                                buildableLocations.Add(new KeyValuePair<string, string>(location.NameOrUniqueName, location.DisplayName));
                            }
                        }
                        if (!buildableLocations.Any())
                        {
                            Farm farm = Game1.getFarm();
                            buildableLocations.Add(new KeyValuePair<string, string>(farm.NameOrUniqueName, farm.DisplayName));
                        }
                        if (Game1.getLocationFromName(__instance.BuilderLocationName) != __instance.TargetLocation)
                        {
                            Game1.playSound("cancel");
                            return false;
                        }
                        Game1.currentLocation.ShowPagedResponses(I18n.Message_AnythingAnywhere_ChooseBuildingLocation(), buildableLocations, delegate (string value)
                        {
                            GameLocation locationFromName = Game1.getLocationFromName(value);

                            if (locationFromName != null)
                            {
                                __instance.TargetLocation = locationFromName;
                                if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu is DialogueBox)
                                {
                                    (Game1.activeClickableMenu as DialogueBox).closeDialogue();
                                }
                                Game1.activeClickableMenu = __instance;

                                __instance.buildingToMove.isMoving = true;
                                Game1.playSound("axchop");
                                Game1.globalFadeToBlack(__instance.setUpForBuildingPlacement);
                            }
                            else
                            {
                                _monitor.Log("Can't find location '" + value + "' for animal relocate menu.", LogLevel.Error);
                            }
                        }, auto_select_single_choice: true, addCancel: false);
                    }
                    return false;
                }
                Vector2 buildingPosition = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
                if (__instance.ConfirmBuildingAccessibility(buildingPosition))
                {
                    if (__instance.TargetLocation.buildStructure(__instance.buildingToMove, buildingPosition, Game1.player))
                    {
                        __instance.buildingToMove.isMoving = false;
                        __instance.buildingToMove = null;
                        Game1.playSound("axchop");
                        DelayedAction.playSoundAfterDelay("dirtyHit", 50);
                        DelayedAction.playSoundAfterDelay("dirtyHit", 150);
                    }
                    else
                    {
                        Game1.playSound("cancel");
                    }
                }
                else
                {
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
                    Game1.playSound("cancel");
                }
                return false;
            }
            return true;
        }






        // Make cabins builable outside of farm if enabled.
        private static bool IsValidBuildingForLocationPrefix(CarpenterMenu __instance, string typeId, BuildingData data, GameLocation targetLocation, ref bool __result)
        {
            if (!ModEntry.modConfig.EnableCabinsAnywhere)
                return true;

            __result = true;
            return false;
        }
    }
}
