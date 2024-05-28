using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using Common.Interfaces;
using System.Collections.Generic;
using Common.Helpers;

namespace AnythingAnywhere
{
    internal class ModConfig : IConfigurable
    {
        // PLACING
        [DefaultValue(true, "Placing")]
        public bool EnablePlacing { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableFreePlace { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableRugRemovalBypass { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableWallFurnitureIndoors { get; set; }

        // BUILDING
        [DefaultValue(true, "Building")]
        public bool EnableBuilding { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableBuildAnywhere { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableInstantBuild { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableFreeBuild { get; set; }

        [DefaultValue(SButton.OemComma, "Building")]
        public KeybindList BuildMenu { get; set; }

        [DefaultValue(SButton.OemPeriod, "Building")]
        public KeybindList WizardBuildMenu { get; set; }

        [DefaultValue(SButton.LeftShift, "Building")]
        public KeybindList BuildModifier { get; set; }

        [DefaultValue(true, "Building")]
        public bool EnableGreenhouse { get; set; }

        [DefaultValue(false, "Building")]
        public bool RemoveBuildConditions { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableBuildingIndoors { get; set; }

        [DefaultValue(false, "Building")]
        public bool BypassMagicInk { get; set; }

        [DefaultValue(null, "Building")]
        public List<string> BlacklistedLocations { get; set; }

        // FARMING
        [DefaultValue(true, "Farming")]
        public bool EnablePlanting { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableDiggingAll { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableFruitTreeTweaks { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableWildTreeTweaks { get; set; }

        // OTHER
        [DefaultValue(true, "Other")]
        public bool EnableAnimalRelocate { get; set; }

        [DefaultValue(false, "Other")]
        public bool EnableCaskFunctionality { get; set; }

        [DefaultValue(false, "Other")]
        public bool EnableJukeboxFunctionality { get; set; }

        [DefaultValue(true, "Other")]
        public bool EnableGoldClockAnywhere { get; set; }

        [DefaultValue(false, "Other")]
        public bool MultipleMiniObelisks { get; set; }

        public ModConfig()
        {
            ConfigUtilities.InitializeDefaultConfig(this);
        }
    }
}