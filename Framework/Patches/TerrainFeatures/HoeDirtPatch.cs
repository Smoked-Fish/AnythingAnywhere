using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class HoeDirtPatch : PatchTemplate
    {
        internal HoeDirtPatch(Harmony harmony) : base(harmony, typeof(HoeDirt)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(HoeDirt.plant), nameof(PlantPrefix), [typeof(string), typeof(Farmer), typeof(bool)]);
        }

        public static bool PlantPrefix(HoeDirt __instance, string itemId, Farmer who, bool isFertilizer, ref bool __result)
        {
            GameLocation location = __instance.Location;
            if (ModEntry.Config.EnablePlanting)
            {
                location.IsFarm = true;
                return true;
            }

            return true;
        }
    }
}
