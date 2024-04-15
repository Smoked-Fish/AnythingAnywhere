using StardewValley;
using StardewModdingAPI;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using Netcode;

namespace AnythingAnywhere.Framework.UI
{
    internal class BuildAnywhereMenu : CarpenterMenu
    {
        // Set to false to prevent errors from the Better Junimos mod
        public bool magicalConstruction = false;
        public BuildAnywhereMenu(string builder, ModConfig config, IMonitor monitor) : base(builder, Game1.currentLocation)
        {
            TargetLocation = Game1.currentLocation;
            int index = 0;
            Blueprints.Clear();
            foreach (KeyValuePair<string, BuildingData> data in Game1.buildingData)
            {

                if ((data.Value.Builder != builder || !GameStateQuery.CheckConditions(data.Value.BuildCondition, TargetLocation) || data.Value.BuildingToUpgrade != null && TargetLocation.getNumberBuildingsConstructed(data.Value.BuildingToUpgrade) == 0 || !IsValidBuildingForLocation(data.Key, data.Value, TargetLocation)) && !config.EnableInstantBuild)
                {
                    continue;
                }
                else if (config.EnableInstantBuild)
                {
                    if (data.Value.Builder != builder)
                        continue;
                    // Create a copy so you don't need to reset game to disable
                    BuildingData copyData = DeepCopy(data.Value);

                    copyData.MagicalConstruction = true;
                    copyData.BuildCost = 0;
                    copyData.BuildDays = 0;
                    copyData.BuildMaterials = [];

                    Blueprints.Add(new BlueprintEntry(index++, data.Key, copyData, null));
                }
                else
                {
                    Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, null));
                }
                /*monitor.LogOnce($"Blueprint Added. Index: {index}\nData: {data.Key}\nData: {data.Value}", LogLevel.Info);*/
                if (data.Value.Skins == null)
                {
                    continue;
                }
                foreach (BuildingSkin skin in data.Value.Skins)
                {
                    if (skin.ShowAsSeparateConstructionEntry && GameStateQuery.CheckConditions(skin.Condition, TargetLocation))
                    {
                        Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, skin.Id));
                    }
                }
            }
            SetNewActiveBlueprint(0);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (freeze)
            {
                return;
            }
            if (!onFarm)
            {
                base.receiveLeftClick(x, y, playSound);
            }
            if (cancelButton.containsPoint(x, y))
            {
                if (onFarm)
                {
                    if (moving && buildingToMove != null)
                    {
                        Game1.playSound("cancel");
                        return;
                    }
                    returnToCarpentryMenu();
                    Game1.playSound("smallSelect");
                    return;
                }
                exitThisMenu();
                Game1.player.forceCanMove();
                Game1.playSound("bigDeSelect");
            }
            if (!onFarm && backButton.containsPoint(x, y))
            {
                SetNewActiveBlueprint(Blueprint.Index - 1);
                Game1.playSound("shwip");
                backButton.scale = backButton.baseScale;
            }
            if (!onFarm && forwardButton.containsPoint(x, y))
            {
                SetNewActiveBlueprint(Blueprint.Index + 1);
                forwardButton.scale = forwardButton.baseScale;
                Game1.playSound("shwip");
            }
            if (!onFarm)
            {
                if (demolishButton.containsPoint(x, y) && demolishButton.visible && CanDemolishThis())
                {
                    Game1.globalFadeToBlack(setUpForBuildingPlacement);
                    Game1.playSound("smallSelect");
                    onFarm = true;
                    demolishing = true;
                }
                if (moveButton.containsPoint(x, y) && moveButton.visible)
                {
                    Game1.globalFadeToBlack(setUpForBuildingPlacement);
                    Game1.playSound("smallSelect");
                    onFarm = true;
                    moving = true;
                }
                if (paintButton.containsPoint(x, y) && paintButton.visible)
                {
                    Game1.globalFadeToBlack(setUpForBuildingPlacement);
                    Game1.playSound("smallSelect");
                    onFarm = true;
                    painting = true;
                }
                if (appearanceButton.containsPoint(x, y) && appearanceButton.visible && currentBuilding.CanBeReskinned(ignoreSeparateConstructionEntries: true))
                {
                    BuildingSkinMenu skinMenu = new BuildingSkinMenu(currentBuilding, ignoreSeparateConstructionEntries: true);
                    Game1.playSound("smallSelect");
                    BuildingSkinMenu buildingSkinMenu = skinMenu;
                    buildingSkinMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(buildingSkinMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
                    {
                        if (Game1.options.SnappyMenus)
                        {
                            setCurrentlySnappedComponentTo(109);
                            snapCursorToCurrentSnappedComponent();
                        }
                        Blueprint.SetSkin(skinMenu.Skin?.Id);
                    });
                    SetChildMenu(skinMenu);
                }
                if (okButton.containsPoint(x, y) && !onFarm && CanBuildCurrentBlueprint())
                {
                    Game1.globalFadeToBlack(setUpForBuildingPlacement);
                    Game1.playSound("smallSelect");
                    onFarm = true;
                }
            }
            if (!onFarm || freeze || Game1.IsFading())
            {
                return;
            }
            GameLocation farm;
            Building destroyed;
            GameLocation interior;
            Cabin cabin;
            if (demolishing)
            {
                farm = TargetLocation;
                destroyed = farm.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
                if (destroyed == null)
                {
                    return;
                }
                interior = destroyed.GetIndoors();
                cabin = interior as Cabin;
                if (destroyed != null)
                {
                    if (cabin != null && !Game1.IsMasterGame)
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
                        destroyed = null;
                        return;
                    }
                    if (!CanDemolishThis(destroyed))
                    {
                        destroyed = null;
                        return;
                    }
                    if (!Game1.IsMasterGame && !hasPermissionsToDemolish(destroyed))
                    {
                        destroyed = null;
                        return;
                    }
                }
                Cabin cabin2 = cabin;
                if (cabin2 != null && cabin2.HasOwner && cabin.owner.isCustomized.Value)
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:Carpenter_DemolishCabinConfirm", cabin.owner.Name), Game1.currentLocation.createYesNoResponses(), delegate (Farmer f, string answer)
                    {
                        if (answer == "Yes")
                        {
                            Game1.activeClickableMenu = this;
                            Game1.player.team.demolishLock.RequestLock(ContinueDemolish, BuildingLockFailed);
                        }
                        else
                        {
                            DelayedAction.functionAfterDelay(returnToCarpentryMenu, 500);
                        }
                    });
                }
                else if (destroyed != null)
                {
                    Game1.player.team.demolishLock.RequestLock(ContinueDemolish, BuildingLockFailed);
                }
                return;
            }
            if (upgrading)
            {
                Building toUpgrade = TargetLocation.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64));
                if (toUpgrade != null && toUpgrade.buildingType.Value == Blueprint.UpgradeFrom)
                {
                    ConsumeResources();
                    toUpgrade.upgradeName.Value = Blueprint.Id;
                    toUpgrade.daysUntilUpgrade.Value = Math.Max(Blueprint.BuildDays, 1);
                    toUpgrade.showUpgradeAnimation(TargetLocation);
                    Game1.playSound("axe");
                    DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 1500);
                    freeze = true;
                    ModEntry.multiplayer.globalChatInfoMessage("BuildingBuild", Game1.player.Name, "aOrAn:" + Blueprint.TokenizedDisplayName, Blueprint.TokenizedDisplayName, Game1.player.farmName.Value);
                    if (Blueprint.BuildDays < 1)
                    {
                        toUpgrade.FinishConstruction();
                    }
                    else
                    {
                        Game1.netWorldState.Value.MarkUnderConstruction(Builder, toUpgrade);
                    }
                }
                else if (toUpgrade != null)
                {
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantUpgrade_BuildingType"), 3));
                }
                return;
            }
            if (painting)
            {
                Vector2 tile_position = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
                Building paint_building = TargetLocation.getBuildingAt(tile_position);
                if (paint_building != null)
                {
                    if (!paint_building.CanBePainted() && !paint_building.CanBeReskinned(ignoreSeparateConstructionEntries: true))
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint"), 3));
                        return;
                    }
                    if (!HasPermissionsToPaint(paint_building))
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CannotPaint_Permission"), 3));
                        return;
                    }
                    paint_building.color = Color.White;
                    SetChildMenu(paint_building.CanBePainted() ? ((IClickableMenu)new BuildingPaintMenu(paint_building)) : ((IClickableMenu)new BuildingSkinMenu(paint_building, ignoreSeparateConstructionEntries: true)));
                }
                return;
            }
            if (moving)
            {
                if (buildingToMove == null)
                {
                    buildingToMove = TargetLocation.getBuildingAt(new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64));
                    if (buildingToMove != null)
                    {
                        if ((int)buildingToMove.daysOfConstructionLeft.Value > 0)
                        {
                            buildingToMove = null;
                            return;
                        }
                        if (!hasPermissionsToMove(buildingToMove))
                        {
                            buildingToMove = null;
                            return;
                        }
                        buildingToMove.isMoving = true;
                        Game1.playSound("axchop");
                    }
                    return;
                }
                Vector2 buildingPosition = new Vector2((Game1.viewport.X + Game1.getMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getMouseY(ui_scale: false)) / 64);
                if (ConfirmBuildingAccessibility(buildingPosition))
                {
                    if (TargetLocation.buildStructure(buildingToMove, buildingPosition, Game1.player))
                    {
                        buildingToMove.isMoving = false;
                        buildingToMove = null;
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
                return;
            }
            Game1.player.team.buildLock.RequestLock(delegate
            {
                if (onFarm && Game1.locationRequest == null)
                {
                    if (tryToBuild())
                    {
                        ConsumeResources();
                        if (!ModEntry.modConfig.EnableInstantBuild)
                        {
                        DelayedAction.functionAfterDelay(returnToCarpentryMenuAfterSuccessfulBuild, 2000);
                        freeze = true;
                        }
                    }
                    else
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantBuild"), 3));
                    }
                }
                Game1.player.team.buildLock.ReleaseLock();
            });
            void BuildingLockFailed()
            {
                if (demolishing)
                {
                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed"), 3));
                }
            }
            void ContinueDemolish()
            {
                if (demolishing && destroyed != null && farm.buildings.Contains(destroyed))
                {
                    if ((int)destroyed.daysOfConstructionLeft.Value > 0 || (int)destroyed.daysUntilUpgrade.Value > 0)
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction"), 3));
                    }
                    else if (interior is AnimalHouse animalHouse && animalHouse.animalsThatLiveHere.Count > 0)
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere"), 3));
                    }
                    else if (interior != null && interior.farmers.Any())
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), 3));
                    }
                    else
                    {
                        if (cabin != null)
                        {
                            foreach (Farmer farmer in Game1.getAllFarmers())
                            {
                                if (farmer.currentLocation != null && farmer.currentLocation.Name == cabin.GetCellarName())
                                {
                                    Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere"), 3));
                                    return;
                                }
                            }
                            if (cabin.IsOwnerActivated)
                            {
                                Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline"), 3));
                                return;
                            }
                        }
                        destroyed.BeforeDemolish();
                        Chest chest = null;
                        if (cabin != null)
                        {
                            List<Item> items = cabin.demolish();
                            if (items.Count > 0)
                            {
                                chest = new Chest(playerChest: true);
                                chest.fixLidFrame();
                                chest.Items.OverwriteWith(items);
                            }
                        }
                        if (farm.destroyStructure(destroyed))
                        {
                            Game1.flashAlpha = 1f;
                            destroyed.showDestroyedAnimation(TargetLocation);
                            Game1.playSound("explosion");
                            Utility.spreadAnimalsAround(destroyed, farm);
                            DelayedAction.functionAfterDelay(returnToCarpentryMenu, 1500);
                            freeze = true;
                            if (chest != null)
                            {
                                farm.objects[new Vector2((int)destroyed.tileX.Value + (int)destroyed.tilesWide.Value / 2, (int)destroyed.tileY.Value + (int)destroyed.tilesHigh.Value / 2)] = chest;
                            }
                        }
                    }
                }
            }
        }

        public new bool tryToBuild()
        {
            NetString skinId = currentBuilding.skinId;
            Vector2 tileLocation = new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64);
            if (TargetLocation.buildStructure(currentBuilding.buildingType.Value, tileLocation, Game1.player, out var building, Blueprint.MagicalConstruction, ModEntry.modConfig.EnableBuildAnywhere))
            {
                building.skinId.Value = skinId.Value;
                if (building.isUnderConstruction())
                {
                    Game1.netWorldState.Value.MarkUnderConstruction(Builder, building);
                }
                return true;
            }
            return false;
        }


        public override bool IsValidBuildingForLocation(string typeId, BuildingData data, GameLocation targetLocation)
        {
            if ((typeId == "Cabin" && TargetLocation.Name != "Farm") && !ModEntry.modConfig.EnableCabinsAnywhere)
                return false;

            return true;
        }


        private BuildingData DeepCopy(BuildingData source)
        {
            if (source == null)
                return null;

            string serializedObject = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<BuildingData>(serializedObject);
        }
    }
}
