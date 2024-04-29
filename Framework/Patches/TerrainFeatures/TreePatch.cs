using HarmonyLib;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using System;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class TreePatch : PatchTemplate
    {
        internal TreePatch(Harmony harmony) : base(harmony, typeof(Tree)) { }
        internal void Apply()
        {
            Patch(true, nameof(Tree.IsGrowthBlockedByNearbyTree), nameof(IsGrowthBlockedByNearbyTreePostfix));
        }

        public static void IsGrowthBlockedByNearbyTreePostfix(Tree __instance, ref bool __result)
        {
            if (ModEntry.modConfig.EnableWildTreeTweaks)
                __result = false;
        }
    }
}
