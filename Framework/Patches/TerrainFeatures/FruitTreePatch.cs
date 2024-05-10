using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;

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
            if (ModEntry.Config.EnableFruitTreeTweaks)
                __result = false;
        }
    }
}
