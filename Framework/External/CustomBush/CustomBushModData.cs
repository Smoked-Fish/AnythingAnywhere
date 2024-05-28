#nullable disable
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

namespace AnythingAnywhere.Framework.External.CustomBush
{
    public static class CustomBushModData
    {
        private const string ModID = "furyx639.CustomBush";
        private const string modDataId = ModID + "/Id";

        public static Bush AddBushModData(Bush bush, SObject __instance)
        {
            if (ModEntry.CustomBushApi != null && ModEntry.ModHelper.ModRegistry.IsLoaded("furyx639.CustomBush"))
            {
                IEnumerable<(string Id, ICustomBush Data)> customBushData = ModEntry.CustomBushApi.GetData();
                if (customBushData.Any(item => item.Id == __instance.QualifiedItemId))
                {
                    bush.modData[modDataId] = __instance.QualifiedItemId;
                    bush.setUpSourceRect();
                }
            }

            return bush;
        }
    }
}
