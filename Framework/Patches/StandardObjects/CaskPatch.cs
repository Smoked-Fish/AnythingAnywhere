#nullable disable
using HarmonyLib;
using StardewValley.Objects;
using Common.Helpers;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal sealed class CaskPatch : PatchHelper
    {
        internal CaskPatch(Harmony harmony) : base(harmony, typeof(Cask)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(Cask.IsValidCaskLocation), nameof(IsValidCaskLocationPrefix));
        }

        // Enable cask functionality outside of the farm
        private static bool IsValidCaskLocationPrefix(Cask __instance, ref bool __result)
        {
            if (ModEntry.Config.EnableCaskFunctionality)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
