using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Tiles;

namespace AnythingAnywhere.Framework.Patches.TerrainFeatures
{
    internal class HoeDirtPatch : PatchTemplate
    {
        private readonly Type _object = typeof(HoeDirt);

        internal HoeDirtPatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }
        internal void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(_object, nameof(HoeDirt.plant),  new[] { typeof(string), typeof(Farmer), typeof(bool) } ), prefix: new HarmonyMethod(GetType(), nameof(PlantPrefix)));
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
