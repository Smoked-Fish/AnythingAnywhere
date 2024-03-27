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
    }

    internal class ModConfigKeys
    {
        public KeybindList ReloadConfig { get; set; } = new();
    }
}
