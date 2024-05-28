#nullable disable
using HarmonyLib;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewValley.Locations;
using Common.Helpers;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal sealed class BedFurniturePatch : PatchHelper
    {
        internal BedFurniturePatch(Harmony harmony) : base(harmony, typeof(BedFurniture)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(BedFurniture.CanModifyBed), nameof(CanModifyBedPostfix), [typeof(Farmer)]);
            Patch(PatchType.Transpiler, nameof(BedFurniture.placementAction), nameof(PlacementActionTranspiler));
        }

        // Enable modifying other players beds/placing inside of other players homes
        private static void CanModifyBedPostfix(BedFurniture __instance, Farmer who, ref bool __result)
        {
            if (!ModEntry.Config.EnableFreePlace)
                __result = true;
        }

        // Enable all beds indoors
        private static IEnumerable<CodeInstruction> PlacementActionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase original)
        {
            try
            {
                var matcher = new CodeMatcher(instructions, generator);

                matcher.MatchEndForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(FarmHouse), nameof(FarmHouse.upgradeLevel)).GetGetMethod()),
                    new CodeMatch(OpCodes.Ldc_I4_2))
                    .Set(OpCodes.Ldc_I4_0, null)
                    .MatchEndForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Property(typeof(FarmHouse), nameof(FarmHouse.upgradeLevel)).GetGetMethod()),
                    new CodeMatch(OpCodes.Ldc_I4_1))
                    .Set(OpCodes.Ldc_I4_0, null)
                    .ThrowIfNotMatch("Could not find get_upgradeLevel()");

                return matcher.InstructionEnumeration();
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"There was an issue modifying the instructions for {typeof(BedFurniture)}.{original.Name}: {e}", LogLevel.Error);
                return instructions;
            }
        }
    }
}