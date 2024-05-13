using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace AnythingAnywhere.Framework.Patches.StandardObjects
{
    internal class BedFurniturePatch : PatchTemplate
    {
        internal BedFurniturePatch(Harmony harmony) : base(harmony, typeof(BedFurniture)) { }
        internal void Apply()
        {
            Patch(PatchType.Postfix, nameof(BedFurniture.CanModifyBed), nameof(CanModifyBedPostfix), [typeof(Farmer)]);
            Patch(PatchType.Transpiler, nameof(BedFurniture.placementAction), nameof(PlacementActionTranspiler));
        }

        // Enable modifying other players beds
        private static void CanModifyBedPostfix(BedFurniture __instance, Farmer who, ref bool __result)
        {
            if (!ModEntry.Config.EnableFreePlace)
                __result = true;
        }


        // Enable all beds indoors
        private static IEnumerable<CodeInstruction> PlacementActionTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            try
            {
                List<CodeInstruction> list = instructions.ToList();

                for (int i = 1; i < list.Count; i++)
                {
                    if (i + 1 < list.Count && list[i].opcode == OpCodes.Callvirt && list[i].operand is MethodInfo methodInfo &&  methodInfo.ReturnType == typeof(int))
                    {
                        list[i + 1].opcode = OpCodes.Ldc_I4_0;
                    }
                }
                return list;
            }
            catch (Exception e)
            {
                ModEntry.ModMonitor.Log($"There was an issue modifying the instructions for {typeof(BedFurniture)}.{original.Name}: {e}", LogLevel.Error);
                return instructions;
            }
        }
    }
}
