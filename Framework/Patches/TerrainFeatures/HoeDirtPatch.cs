using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;
using System;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class HoeDirtPatch : PatchTemplate
    {
        private readonly Type _object = typeof(HoeDirt);

        internal HoeDirtPatch(Harmony harmony) : base(harmony)
        {
            
        }
        internal void Apply()
        {
            _harmony.Patch(AccessTools.Method(_object, nameof(HoeDirt.plant),  new[] { typeof(string), typeof(Farmer), typeof(bool) } ), prefix: new HarmonyMethod(GetType(), nameof(PlantPrefix)));
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
