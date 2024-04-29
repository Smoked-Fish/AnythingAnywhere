using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;


namespace AnythingAnywhere.Framework.Patches
{
    internal class PatchTemplate
    {
        internal static Harmony _harmony;
        
        internal PatchTemplate(Harmony modHarmony)
        {
            _harmony = modHarmony;
        }
    }
}
