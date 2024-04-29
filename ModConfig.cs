using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AnythingAnywhere
{
    internal class ModConfig
    {
       
        // PLACING
        public bool EnablePlacing { get; set; } = true;
        public bool EnableFreePlace { get; set; } = false;
        public bool EnableWallFurnitureIndoors { get; set; } = false;
        public bool EnableRugRemovalBypass { get; set; } = false;


        // BUILDING
        public bool EnableBuilding { get; set; } = true;
        public bool EnableBuildAnywhere { get; set; } = false;
        public bool EnableInstantBuild { get; set; } = false;
        public KeybindList BuildMenu { get; set; } = new KeybindList(SButton.OemComma);
        public KeybindList WizardBuildMenu { get; set; } = new KeybindList(SButton.OemPeriod);
        public KeybindList BuildModifier { get; set; } = new KeybindList(SButton.LeftShift);
        public bool EnableGreenhouse { get; set; } = true;
        public bool RemoveBuildConditions { get; set; } = false;
        public bool EnableBuildingIndoors { get; set; } = false;
        public bool BypassMagicInk { get; set; } = false;

        // FARMING
        public bool EnablePlanting { get; set; } = true;
        public bool EnableDiggingAll { get; set; } = false;
        public bool EnableFruitTreeTweaks { get; set; } = false;
        public bool EnableWildTreeTweaks { get; set; } = false;

        // OTHER
        public bool EnableAnimalRelocate { get; set; } = true;
        public bool EnableCaskFunctionality { get; set; } = false;
        public bool EnableJukeboxFunctionality { get; set; } = true;
        public bool EnableGoldClockAnywhere { get; set; } = true;
        public bool MultipleMiniObelisks { get; set; } = false;
        public bool EnableCabinsAnywhere { get; set; } = false;
    }
}
