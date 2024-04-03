using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AnythingAnywhere
{
    internal class ModConfig
    {
        public ModConfigKeys Keys { get; set; } = new();
        
        // FURNITURE
        public bool EnablePlacing { get; set; } = true;
        //public bool EnableRugTweaks { get; set; } = true;
        public bool EnableWallFurnitureIndoors { get; set; } = false;
        public bool EnableFreePlace { get; set; } = false;


        // BUILDING
        public bool EnableBuilding { get; set; } = true;
        public bool EnableBuildingIndoors { get; set; } = false;
        public KeybindList BuildMenu { get; set; } = new KeybindList(SButton.OemComma);
        public KeybindList WizardBuildMenu { get; set; } = new KeybindList(SButton.OemPeriod);
        public bool EnableAnimalRelocate { get; set; } = true;
        public bool EnableInstantBuild { get; set; } = false;
        public bool EnableBuildAnywhere { get; set; } = false;


        // OTHER
        //public KeybindList TableTweakBind { get; set; } = new KeybindList(SButton.LeftShift);
        //public bool EnableTableTweak {  get; set; } = true;
        public bool EnablePlanting { get; set; } = true;
        public bool EnableDiggingAll { get; set; } = false;
        public bool EnableFruitTreeTweaks { get; set; } = true;
        public bool EnableWildTreeTweaks { get; set; } = false;
        public bool BypassMagicInk { get; set; } = false;
        public bool MultipleMiniObelisks { get; set; } = false;
        public bool EnableJukeboxFunctionality { get; set; } = true;
    }

    internal class ModConfigKeys
    {
        public KeybindList ReloadConfig { get; set; } = new();
    }
}
