using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;

namespace AnythingAnywhere
{
    internal class ModConfig
    {
        public ModConfigKeys Keys { get; set; } = new();
        public bool AllowAllGroundFurniture { get; set; } = true;
        public bool AllowAllWallFurniture { get; set; } = true;
        public bool AllowMiniObelisksAnywhere { get; set; } = true;
        public bool EnableJukeboxFunctionality { get; set; } = true;
        public bool AllowAllWallFurnitureFarmHouse { get; set; } = false;
    }

    internal class ModConfigKeys
    {
        public KeybindList ReloadConfig { get; set; } = new();
    }
}
