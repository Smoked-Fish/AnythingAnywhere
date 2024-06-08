using Common.Helpers;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AnythingAnywhere.Framework.Patches
{
    internal sealed class MiscPatches : PatchHelper
    {
        public void Apply()
        {
            Patch<Cask>(PatchType.Prefix, nameof(Cask.IsValidCaskLocation), nameof(IsValidCaskLocationPrefix));
            Patch<MiniJukebox>(PatchType.Prefix, nameof(MiniJukebox.checkForAction), nameof(CheckForActionPrefix), [typeof(Farmer), typeof(bool)]);
            Patch<GameLocation>(PatchType.Prefix, nameof(GameLocation.spawnWeedsAndStones), nameof(SpawnWeedsAndStonesPrefix), [typeof(int), typeof(bool), typeof(bool)]);
            Patch<GameLocation>(PatchType.Prefix, nameof(GameLocation.loadWeeds), nameof(LoadWeedsPrefix));
        }

        // Enable cask functionality outside of the farm
        private static bool IsValidCaskLocationPrefix(Cask __instance, ref bool __result)
        {
            if (!ModEntry.Config.EnableCaskFunctionality) return true;

            __result = true;
            return false;
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

        private static bool SpawnWeedsAndStonesPrefix(GameLocation __instance, int numDebris = -1, bool weedsOnly = false, bool spawnFromOldWeeds = true)
        {
            if (!ModEntry.Config.EnableGoldClockAnywhere)
                return true;

            bool hasGoldClock = __instance.buildings.Any(building => building.buildingType.Value == "Gold Clock");
            return !hasGoldClock || Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
        }

        private static bool LoadWeedsPrefix(GameLocation __instance)
        {
            if (!ModEntry.Config.EnableGoldClockAnywhere)
                return true;

            bool hasGoldClock = __instance.buildings.Any(building => building.buildingType.Value == "Gold Clock");
            return !hasGoldClock || Game1.netWorldState.Value.goldenClocksTurnedOff.Value;
        }
    }
}
