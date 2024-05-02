using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using System;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class FruitTreePatch : PatchTemplate
    {
        internal FruitTreePatch(Harmony harmony) : base(harmony, typeof(FruitTree)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(FruitTree.IsGrowthBlocked), nameof(IsGrowthBlockedPostfix), [typeof(Vector2), typeof(GameLocation)]);
        }

        public static void IsGrowthBlockedPostfix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result)
        {
            if (ModEntry.modConfig.EnableFruitTreeTweaks)
                __result = false;
        }
    }
}
