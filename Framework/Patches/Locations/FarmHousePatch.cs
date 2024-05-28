#nullable disable
using HarmonyLib;
using StardewValley;
using StardewValley.Locations;
using Microsoft.Xna.Framework;
using Common.Helpers;

namespace AnythingAnywhere.Framework.Patches.Locations
{
    internal sealed class FarmHousePatch : PatchHelper
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
