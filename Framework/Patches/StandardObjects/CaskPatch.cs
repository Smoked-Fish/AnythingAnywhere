#nullable disable
using Common.Helpers;
using StardewValley.Objects;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal sealed class CaskPatch : PatchHelper
    {
        internal CaskPatch() : base(typeof(Cask)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(Cask.IsValidCaskLocation), nameof(IsValidCaskLocationPrefix));
        }

        // Enable cask functionality outside of the farm
        private static bool IsValidCaskLocationPrefix(Cask __instance, ref bool __result)
        {
            if (!ModEntry.Config.EnableCaskFunctionality) return true;

            __result = true;
            return false;
        }
    }
}
