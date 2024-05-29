using Common.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace AnythingAnywhere.Framework.Patches.Locations
{
    internal sealed class FarmHousePatch : PatchHelper
    {
        public static Vector2 FarmHouseRealPos {  get; private set; }
        internal FarmHousePatch() : base(typeof(FarmHouse)) { }
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
