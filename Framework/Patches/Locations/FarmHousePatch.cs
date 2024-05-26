using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using Common.Util;

namespace AnythingAnywhere.Framework.Patches.Locations
{
    internal class FarmHousePatch : PatchTemplate
    {
        public static Vector2 FarmHouseRealPos {  get; set; }
        internal FarmHousePatch(Harmony harmony) : base(harmony, typeof(FarmHouse)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, "resetLocalState", nameof(ResetLocalStatePrefix));
        }

        private static void ResetLocalStatePrefix(FarmHouse __instance)
        {
            FarmHouseRealPos = Game1.player.Tile;
        }
    }
}
