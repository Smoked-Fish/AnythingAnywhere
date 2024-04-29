using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewValley.Menus;
using System.Collections.Generic;
using System;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal class CaskPatch : PatchTemplate
    {
        private readonly Type _object = typeof(Cask);

        internal CaskPatch(Harmony harmony) : base(harmony)
        {

        }
        internal void Apply()
        {
            _harmony.Patch(AccessTools.Method(_object, nameof(Cask.IsValidCaskLocation)), prefix: new HarmonyMethod(GetType(), nameof(IsValidCaskLocationPrefix)));
        }

        // Enable jukebox functionality outside of the farm
        private static bool IsValidCaskLocationPrefix(Cask __instance, ref bool __result)
        {
            if (ModEntry.modConfig.EnableCaskFunctionality)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
