#nullable disable
using Common.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal sealed class FruitTreePatch : PatchHelper
    {
        internal FruitTreePatch() : base(typeof(FruitTree)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(FruitTree.IsGrowthBlocked), nameof(IsGrowthBlockedPostfix), [typeof(Vector2), typeof(GameLocation)]);
            Patch(PatchType.Postfix, nameof(FruitTree.IsTooCloseToAnotherTree), nameof(IsTooCloseToAnotherTreePostfix), [typeof(Vector2), typeof(GameLocation), typeof(bool)]);
        }

        public static void IsGrowthBlockedPostfix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result)
        {
            if (ModEntry.Config.EnableFruitTreeTweaks)
                __result = false;
        }

        private static void IsTooCloseToAnotherTreePostfix(FruitTree __instance, Vector2 tileLocation, GameLocation environment, ref bool __result, bool fruitTreesOnly = false)
        {
            if (ModEntry.Config.EnableFruitTreeTweaks)
                __result = false;
        }
    }
}