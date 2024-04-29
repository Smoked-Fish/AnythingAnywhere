using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class HoeDirtPatch : PatchTemplate
    {
        internal HoeDirtPatch(Harmony harmony) : base(harmony, typeof(HoeDirt)) { }
        internal void Apply()
        {
            Patch(false, nameof(HoeDirt.plant), nameof(PlantPrefix), [typeof(string), typeof(Farmer), typeof(bool)]);
        }

        public static bool PlantPrefix(HoeDirt __instance, string itemId, Farmer who, bool isFertilizer, ref bool __result)
        {
            GameLocation location = __instance.Location;
            if (ModEntry.modConfig.EnablePlanting)
            {
                location.IsFarm = true;
                return true;
            }

            return true;
        }
    }
}
