using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.SDKs;
using System.Collections.Generic;
using System.Linq;
using System;
using Patches = AnythingAnywhere.Features.Patches;
using AnythingAnywhere.Features;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System.Xml.Linq;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;

namespace AnythingAnywhere
{
    internal sealed class ModEntry : Mod
    {
        private ModConfig Config { get; set; }

        private ModConfigKeys Keys => this.Config.Keys;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);


            this.Config = helper.ReadConfig<ModConfig>();
            Harmony harmony = new(this.ModManifest.UniqueID);
            Patches.Initialise(harmony, this.Monitor, () => this.Config, this.Helper.Reflection);

            helper.ConsoleCommands.Add("seed_packet_fix", "Seed packet placement fix.\n\nUsage: seed_packet_fix <value>\n- value: the map id. \"current\" for current map.", this.SeedPacketFix);

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        }

        private void SeedPacketFix(string command, string[] args)
        {

            HandleCommand(this.Monitor, args[0], args[1]);
        }


        private int RemoveCustomObjects(GameLocation location, string itemToRemove)
        {
            int removed = 0;

            foreach ((Vector2 tile, SObject? obj) in location.Objects.Pairs.ToArray())
            {
                if (obj.QualifiedItemId == itemToRemove)
                {
                    location.Objects.Remove(tile);
                    removed++;
                }
            }

            return removed;
        }


        public void HandleCommand(IMonitor monitor, string reqLoc, string objToRemove)
        {
            // check context
            if (!Context.IsWorldReady)
            {
                monitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            // get target location
            GameLocation? location = Game1.locations.FirstOrDefault(p => p.Name != null && p.Name.Equals(reqLoc, StringComparison.OrdinalIgnoreCase));
            if (location == null && reqLoc == "current")
                location = Game1.currentLocation;
            if (location == null)
            {
                string[] locationNames = (from loc in Game1.locations where !string.IsNullOrWhiteSpace(loc.Name) orderby loc.Name select loc.Name).ToArray();
                monitor.Log($"Could not find a location with that name. Must be one of [{string.Join(", ", locationNames)}].", LogLevel.Error);
                return;
            }

            this.RemoveCustomObjects(location, objToRemove);
        }



        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new ModInteractions.GenericModConfigMenu(this.Helper.ModRegistry, this.ModManifest, this.Monitor, () => this.Config, () => this.Config = new ModConfig(), () => this.Helper.WriteConfig(this.Config)).Register();
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (this.Keys.ReloadConfig.JustPressed())
                this.ReloadConfig();

            if (this.Config.BuildMenu.JustPressed() && Config.EnableBuilding)
            {
                HandleInstantBuildButtonClick("Robin");
            }

            if (this.Config.WizardBuildMenu.JustPressed() && Config.EnableBuilding)
            {
                HandleInstantBuildButtonClick("Wizard");
            }
        }

        private void HandleInstantBuildButtonClick(string builder)
        {
            if (Context.IsPlayerFree && Game1.activeClickableMenu == null)
            {
                activateBuildAnywhereMenu(builder);
            }
            else if (Game1.activeClickableMenu is BuildAnywhereMenu)
            {
                Game1.displayFarmer = true;
                ((BuildAnywhereMenu)Game1.activeClickableMenu).returnToCarpentryMenu();
                ((BuildAnywhereMenu)Game1.activeClickableMenu).exitThisMenu();
            }
        }

        private void activateBuildAnywhereMenu(string builder)
        {
            if (builder == "Wizard" && !Game1.getFarmer(Game1.player.UniqueMultiplayerID).hasMagicInk && !Config.EnableFreeBuild)
            {
                string message = I18n.Message_AnythingAnywhere_NoMagicInk();
                Game1.addHUDMessage(new HUDMessage(message, HUDMessage.error_type) { timeLeft = HUDMessage.defaultTime });
                return;
            }
            else if (!Config.EnableBuilding)
            {
                return;
            }
            else
            {
                Game1.activeClickableMenu = (IClickableMenu)new BuildAnywhereMenu(builder, this.Config, this.Monitor);
            }
        }

        private void ReloadConfig()
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.Monitor.Log(I18n.Message_ConfigReloaded(), LogLevel.Info);
        }
    }
}
