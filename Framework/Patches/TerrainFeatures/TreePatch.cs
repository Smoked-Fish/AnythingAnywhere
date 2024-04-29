using HarmonyLib;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using System;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class TreePatch : PatchTemplate
    {
        private readonly Type _object = typeof(Tree);

        internal TreePatch(Harmony harmony) : base(harmony)
        {

        }
        internal void Apply()
        {
            _harmony.Patch(AccessTools.Method(_object, nameof(Tree.IsGrowthBlockedByNearbyTree)), postfix: new HarmonyMethod(GetType(), nameof(IsGrowthBlockedByNearbyTreePostfix)));
        }

        public static void IsGrowthBlockedByNearbyTreePostfix(Tree __instance, ref bool __result)
        {
            if (ModEntry.modConfig.EnableWildTreeTweaks)
                __result = false;
        }
    }
}
