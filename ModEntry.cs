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


            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
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

            if (this.Config.BuildMenu.JustPressed())
            {
                HandleInstantBuildButtonClick("Robin");
            }

            if (this.Config.WizardBuildMenu.JustPressed())
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
            Game1.activeClickableMenu = (IClickableMenu)new BuildAnywhereMenu(builder);
        }

        private void ReloadConfig()
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.Monitor.Log(I18n.Message_ConfigReloaded(), LogLevel.Info);
        }
    }
}
