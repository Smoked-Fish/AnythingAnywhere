using HarmonyLib;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using System.Collections.Generic;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal class MiniJukeboxPatch : PatchTemplate
    {
        internal MiniJukeboxPatch(Harmony harmony) : base(harmony, typeof(MiniJukebox)) { }
        internal void Apply()
        {
            Patch(PatchType.Prefix, nameof(MiniJukebox.checkForAction), nameof(CheckForActionPrefix), [typeof(Farmer), typeof(bool)]);
        }

        // Enable jukebox functionality outside of the farm
        private static bool CheckForActionPrefix(MiniJukebox __instance, Farmer who, ref bool __result, bool justCheckingForActivity = false)
        {
            if (!ModEntry.Config.EnableJukeboxFunctionality)
            {
                return true; //run the original method
            }
            if (justCheckingForActivity)
            {
                __result = true;
                return false;
            }
            GameLocation location = __instance.Location;
            if (location.IsOutdoors && location.IsRainingHere())
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_OutdoorRainy"));
            }
            else
            {
                List<string> jukeboxTracks = Utility.GetJukeboxTracks(Game1.player, Game1.player.currentLocation);
                jukeboxTracks.Insert(0, "turn_off");
                jukeboxTracks.Add("random");
                Game1.activeClickableMenu = new ChooseFromListMenu(jukeboxTracks, __instance.OnSongChosen, isJukebox: true, location.miniJukeboxTrack.Value);
            }
            __result = true;
            return false;
        }
    }
}
