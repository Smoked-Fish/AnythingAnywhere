using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData;
using StardewValley.Inventories;
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

            // Furniture

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
                prefix: new HarmonyMethod(typeof(Patches), nameof(placementActionObject_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Object.placementActionObject_prefix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.placementAction), [typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(placementActionFurniture_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Furniture.placementActionObject_prefix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.canBePlacedHere), [typeof(GameLocation), typeof(Vector2), typeof(CollisionMask), typeof(bool)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(canBePlacedHereFurniture_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Furniture.canBePlacedHereFurniture_postfix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.canBePlacedHere), [typeof(GameLocation), typeof(Vector2), typeof(CollisionMask), typeof(bool)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(canBePlacedHereObject_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"StardewValley.Object.canBePlacedHereObject_postfix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.canBeRemoved), [typeof(Farmer)]),
                postfix: new HarmonyMethod(typeof(Patches), nameof(canBeRemoved_postfix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Furniture.canBeRemoved_postfix\".", LogLevel.Trace);

            s_harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.clicked), [typeof(Farmer)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(clicked_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"Furniture.clicked_prefix\".", LogLevel.Trace);

            // Building

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

            // OTHER

            s_harmony.Patch(
                original: AccessTools.Method(typeof(MiniJukebox), nameof(MiniJukebox.checkForAction), [typeof(Farmer), typeof(bool)]),
                prefix: new HarmonyMethod(typeof(Patches), nameof(checkForAction_prefix))
            );
            s_monitor.Log($"Applying Harmony patch: prefixing method \"MiniJukebox.checkForAction\".", LogLevel.Trace);

            Applied = true;
        }



/*        private static bool dayUpdate_prefix(JunimoHut __instance, int dayOfMonth, ref bool __result)
        {
            try
            {
                Building building = new Building();
                building.dayUpdate(dayOfMonth);
                __instance.myJunimos.Clear();
                __instance.wasLit.Value = false;
                __instance.shouldSendOutJunimos.Value = true;
                __result = true;
                if (__instance.raisinDays.Value > 0 && !Game1.IsWinter)
                {
                    __instance.raisinDays.Value--;
                }
                if ((int)__instance.raisinDays.Value == 0 && !Game1.IsWinter)
                {
                    Chest output = __instance.GetOutputChest();
                    if (output.Items.CountId("(O)Raisins") > 0)
                    {
                        __instance.raisinDays.Value += 7;
                        output.Items.ReduceId("(O)Raisins", 1);
                    }
                }
                foreach (Farmer f in Game1.getAllFarmers())
                {
                    if (f.isActive() && f.currentLocation != null && (f.currentLocation is FarmHouse))
                    {
                        __instance.shouldSendOutJunimos.Value = false;
                        __result = false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"dayUpdate_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true;
            }
        }*/








        // Table Tweak
        private static bool clicked_prefix(Furniture __instance, Farmer who, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();
                if (!config.EnableFurniture)
                    return true;


                if ((__instance.furniture_type.Value == Furniture.table || __instance.furniture_type.Value == Furniture.longTable) && !config.TableTweakBind.IsDown())
                {
                    if (__instance.heldObject.Value != null)
                    {
                        string message = I18n.Message_AnythingAnywhere_TableRemoval(keybind: config.TableTweakBind);
                        Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                    }
                    __result = false;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"clicked_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true;
            }
        }

        private static bool placementActionFurniture_prefix(Furniture __instance, GameLocation location, int x, int y, ref bool __result, Farmer who = null)
        {
            try
            {
                ModConfig config = s_config();

                if (!config.EnableFurniture)
                    return true;

                StardewValley.Object toPlace = (StardewValley.Object)__instance.getOne();
                if (config.EnableFurniture)
                {
                    foreach (Furniture f in location.furniture)
                    {
                        if (f.furniture_type.Value == 11 && (toPlace is Furniture) && !config.TableTweakBind.IsDown())
                        {
                            string message = I18n.Message_AnythingAnywhere_TableAddition(keybind: config.TableTweakBind);
                            Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                            __result = false;
                            return false; //skip original method
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"placementActionFurniture_prefix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return true; //run the original method
            }
        }




        // Furniture

        private static void CanPlaceThisFurnitureHere_postfix(GameLocation __instance, Furniture furniture, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();
                if (config.EnableFurniture)
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

                // Check if the furniture is wall furniture
                bool isWallFurniture =
                    (__instance.furniture_type.Value == 6 ||
                    __instance.furniture_type.Value == 17 ||
                    __instance.furniture_type.Value == 13 ||
                    __instance.QualifiedItemId == "(F)1293");

                // Check conditions for running the code inside
                if (!config.EnableWallFurnitureIndoors && location is DecoratableLocation decoratableLocation && !config.EnableFreeBuild)
                {
                    // Conditions met, but skip if it's not wall furniture
                    if (!isWallFurniture)
                    {
                        __result = 0;
                    }
                    return;
                }
                // If EnableFreeBuild or EnableWallFurnitureIndoors are true
                else
                {
                    __result = 0;
                }
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"GetAdditionalFurniturePlacementStatus_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }

        private static bool placementActionObject_prefix(StardewValley.Object __instance, GameLocation location, int x, int y, ref bool __result, Farmer who = null)
        {
            try
            {
                ModConfig config = s_config();
                if (!config.EnableFurniture)
                    return true;

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

                if (config.EnableFreePlace)
                    __result = true;

                if (__result)
                {
                    if (__instance.Category == -19 && location.terrainFeatures.TryGetValue(placementTile, out var terrainFeature3) && terrainFeature3 is HoeDirt { crop: not null } dirt3 && (__instance.QualifiedItemId == "(O)369" || __instance.QualifiedItemId == "(O)368") && (int)dirt3.crop.currentPhase.Value != 0)
                    {
                        Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
                        __result = false;
                        return false;
                    }
                    if (__instance.isSapling())
                    {
                        if (__instance.IsWildTreeSapling() || __instance.IsFruitTreeSapling())
                        {
                            if (FruitTree.IsTooCloseToAnotherTree(new Vector2(x / 64, y / 64), location))
                            {
                                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
                                __result = false;
                                return false;
                            }
                            if (FruitTree.IsGrowthBlocked(new Vector2(x / 64, y / 64), location))
                            {
                                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", __instance.DisplayName));
                                __result = false;
                                return false;
                            }
                        }
                        if (location.terrainFeatures.TryGetValue(placementTile, out var terrainFeature2))
                        {
                            if (!(terrainFeature2 is HoeDirt { crop: null }))
                            {
                                __result = false;
                                return false;
                            }
                            location.terrainFeatures.Remove(placementTile);
                        }
                        string deniedMessage2 = null;
                        bool canDig = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null;
                        string tileType = location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Type", "Back");
                        bool canPlantTrees = location.doesEitherTileOrTileIndexPropertyEqual((int)placementTile.X, (int)placementTile.Y, "CanPlantTrees", "Back", "T");
                        if ((location is Farm && (canDig || tileType == "Grass" || tileType == "Dirt" || canPlantTrees) && (!location.IsNoSpawnTile(placementTile, "Tree") || canPlantTrees)) || ((canDig || tileType == "Stone") && location.CanPlantTreesHere(__instance.ItemId, (int)placementTile.X, (int)placementTile.Y, out deniedMessage2)))
                        {
                            location.playSound("dirtyHit");
                            DelayedAction.playSoundAfterDelay("coin", 100);
                            if (__instance.IsTeaSapling())
                            {
                                location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location));
                                __result = true;
                                return false;
                            }
                            FruitTree fruitTree = new FruitTree(__instance.ItemId)
                            {
                                GreenHouseTileTree = (location.IsGreenhouse && tileType == "Stone")
                            };
                            fruitTree.growthRate.Value = Math.Max(1, __instance.Quality + 1);
                            location.terrainFeatures.Add(placementTile, fruitTree);
                            __result = true;
                            return false;
                        }
                        if (deniedMessage2 == null)
                        {
                            deniedMessage2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068");
                        }
                        Game1.showRedMessage(deniedMessage2);
                        __result = false;
                        return false;
                    }
                    if (__instance.Category == -74 || __instance.Category == -19)
                    {
                        if (location.terrainFeatures.TryGetValue(placementTile, out var terrainFeature) && terrainFeature is HoeDirt dirt2)
                        {
                            string seedId = Crop.ResolveSeedId(who.ActiveObject.ItemId, location);
                            if (dirt2.canPlantThisSeedHere(seedId, who.ActiveObject.Category == -19))
                            {
                                if (dirt2.plant(seedId, who, who.ActiveObject.Category == -19) && who.IsLocalPlayer)
                                {
                                    if (__instance.Category == -74)
                                    {
                                        foreach (StardewValley.Object o in location.Objects.Values)
                                        {
                                            if (!o.IsSprinkler() || o.heldObject.Value == null || !(o.heldObject.Value.QualifiedItemId == "(O)913") || !o.IsInSprinklerRangeBroadphase(placementTile))
                                            {
                                                continue;
                                            }
                                            if (!o.GetSprinklerTiles().Contains(placementTile))
                                            {
                                                continue;
                                            }
                                            StardewValley.Object value = o.heldObject.Value.heldObject.Value;
                                            Chest chest = value as Chest;
                                            if (chest == null)
                                            {
                                                continue;
                                            }
                                            IInventory items = chest.Items;
                                            if (items.Count <= 0 || items[0] == null || chest.GetMutex().IsLocked())
                                            {
                                                continue;
                                            }
                                            chest.GetMutex().RequestLock(delegate
                                            {
                                                if (items.Count > 0 && items[0] != null)
                                                {
                                                    Item item = items[0];
                                                    if (item.Category == -19 && ((HoeDirt)terrainFeature).plant(item.ItemId, who, isFertilizer: true))
                                                    {
                                                        item.Stack--;
                                                        if (item.Stack <= 0)
                                                        {
                                                            items[0] = null;
                                                        }
                                                    }
                                                }
                                                chest.GetMutex().ReleaseLock();
                                            });
                                            break;
                                        }
                                    }
                                    Game1.haltAfterCheck = false;
                                    __result = true;
                                    return false;
                                }
                                __result = false;
                                return false;
                            }
                            __result = false;
                            return false;
                        }
                        __result = false;
                        return false;
                    }




                    StardewValley.Object toPlace = (StardewValley.Object)__instance.getOne();
                    bool place_furniture_instance_instead = false;
                    if (toPlace.GetType() == typeof(Furniture) && Furniture.GetFurnitureInstance(__instance.ItemId, new Vector2(x / 64, y / 64)).GetType() != toPlace.GetType())
                    {
                        StorageFurniture storageFurniture = new StorageFurniture(__instance.ItemId, new Vector2(x / 64, y / 64));
                        storageFurniture.currentRotation.Value = (__instance as Furniture).currentRotation.Value;
                        storageFurniture.updateRotation();
                        toPlace = storageFurniture;
                        place_furniture_instance_instead = true;
                    }
                    toPlace.shakeTimer = 50;
                    toPlace.Location = location;
                    toPlace.TileLocation = placementTile;
                    toPlace.performDropDownAction(who);
                    if (toPlace.QualifiedItemId == "(BC)TextSign")
                    {
                        toPlace.signText.Value = null;
                        toPlace.showNextIndex.Value = true;
                    }
                    if (toPlace.name.Contains("Seasonal"))
                    {
                        int baseIndex = toPlace.ParentSheetIndex - toPlace.ParentSheetIndex % 4;
                        toPlace.ParentSheetIndex = baseIndex + location.GetSeasonIndex();
                    }
                    if (!(toPlace is Furniture) && !config.EnableFreePlace && location.objects.TryGetValue(placementTile, out var tileObj))
                    {
                        if (tileObj.QualifiedItemId != __instance.QualifiedItemId)
                        {
                            Game1.createItemDebris(tileObj, placementTile * 64f, Game1.random.Next(4));
                            location.objects[placementTile] = toPlace;
                        }
                    }
                    else if (toPlace is Furniture furniture)
                    {
                        if (place_furniture_instance_instead)
                        {
                            location.furniture.Add(furniture);
                        }
                        else
                        {
                            location.furniture.Add(__instance as Furniture);
                        }
                    }
                    else
                    {
                        location.objects.Add(placementTile, toPlace);
                    }
                    toPlace.initializeLightSource(placementTile);

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




        private static void canBePlacedHereObject_postfix(StardewValley.Object __instance, GameLocation l, Vector2 tile, ref bool __result, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
        {
            try
            {
                // Enables placing objects in walls.
                ModConfig config = s_config();
                if (config.EnableFreePlace)
                    __result = true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"canBePlacedHereObject_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
            }
        }


        private static void canBePlacedHereFurniture_postfix(Furniture __instance, GameLocation l, Vector2 tile, ref bool __result, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
        {
            try
            {
                //Enable placing furniture in walls
                ModConfig config = s_config();
                if (config.EnableFreePlace)
                    __result = true;


                if (config.EnableFurniture && __instance.furniture_type.Value == 12 && config.EnableRugTweaks)
                    __result = true;
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"canBePlacedHereFurniture_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
            }
        }

        // Enable jukebox functionality outside of the farm
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

        // Enable picking up rugs with furniture on top.
        private static void canBeRemoved_postfix(Furniture __instance, Farmer who, ref bool __result)
        {
            try
            {
                ModConfig config = s_config();

                if (!config.EnableRugTweaks || __instance.furniture_type.Value != 12)
                    return;

                foreach (Furniture f in who.currentLocation.furniture)
                {
                    if (f.furniture_type.Value == 12)
                    {
                        __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                s_monitor.LogOnce($"Harmony patch \"IsBuildableLocation_postfix\" has encountered an error. Full error message: \n{ex.ToString()}", LogLevel.Error);
                return;
            }
        }


        // Buildings

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