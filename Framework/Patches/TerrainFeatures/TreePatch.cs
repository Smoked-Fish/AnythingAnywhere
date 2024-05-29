using Common.Helpers;
using StardewValley.TerrainFeatures;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal sealed class TreePatch : PatchHelper
    {
        internal TreePatch() : base(typeof(Tree)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(Tree.IsGrowthBlockedByNearbyTree), nameof(IsGrowthBlockedByNearbyTreePostfix));
        }

        public static void IsGrowthBlockedByNearbyTreePostfix(Tree __instance, ref bool __result)
        {
            if (ModEntry.Config.EnableWildTreeTweaks)
                __result = false;
        }
    }
}
