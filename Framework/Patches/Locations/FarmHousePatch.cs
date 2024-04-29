using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Locations;
using StardewValley.Minigames;
using Microsoft.Xna.Framework;

namespace AnythingAnywhere.Framework.Patches.Locations
{
    internal class FarmHousePatch : PatchTemplate
    {
        internal FarmHousePatch(Harmony harmony) : base(harmony, typeof(FarmHouse)) { }
        internal void Apply()
        {
            Patch(false, "resetLocalState", nameof(ResetLocalStatePrefix));
        }

        private static bool ResetLocalStatePrefix(FarmHouse __instance)
        {
            if (Game1.player.currentLocation.Name.StartsWith("ScienceHouse") || Game1.player.currentLocation.Name.EndsWith("ScienceHouse"))
                return true;

            if (!Game1.player.currentLocation.IsOutdoors && !(Game1.player.currentLocation is FarmHouse || Game1.player.currentLocation is Cabin))
            {
                //Game1.player.Position = Utility.PointToVector2(getEntryLocation()) * 64f;
                Game1.xLocationAfterWarp = Game1.player.TilePoint.X;
                Game1.yLocationAfterWarp = Game1.player.TilePoint.Y;
                Game1.player.currentLocation = __instance;
                return false;
            }
            return true;
        }
    }
}
