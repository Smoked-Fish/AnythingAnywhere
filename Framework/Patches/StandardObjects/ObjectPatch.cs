using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Object = StardewValley.Object;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal class ObjectPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Object);

        internal ObjectPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(Object.placementAction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(PlacementActionPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Object.canBePlacedHere), new[] { typeof(GameLocation), typeof(Vector2), typeof(CollisionMask), typeof(bool) }), postfix: new HarmonyMethod(GetType(), nameof(CanBePlacedHerePostfix)));
        }

        // Lets the placement of some special items including the mini fridge and obelisk
        // NEED TO FIX
        private static bool PlacementActionPrefix(Object __instance, GameLocation location, int x, int y, ref bool __result, Farmer who = null)
        {
            if (!ModEntry.modConfig.EnableFurniture)
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
                    if (!ModEntry.modConfig.AllowMiniObelisksAnywhere)
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

            if (ModEntry.modConfig.EnableFreePlace)
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
                if (!(toPlace is Furniture) && !ModEntry.modConfig.EnableFreePlace && location.objects.TryGetValue(placementTile, out var tileObj))
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

        // Lets objects be placed inside of walls
        // NEED TO FIX
        private static void CanBePlacedHerePostfix(Object __instance, GameLocation l, Vector2 tile, ref bool __result, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
        {
            if (ModEntry.modConfig.EnableFreePlace)
                __result = true;
        }

    }
}
