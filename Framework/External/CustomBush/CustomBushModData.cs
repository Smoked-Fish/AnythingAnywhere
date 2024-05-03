using AnythingAnywhere.Framework.Interfaces;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AnythingAnywhere.Framework.External.CustomBush
{
    public class CustomBushModData
    {
        private static string ModID = "furyx639.CustomBush";
        private static string modDataId = ModID + "/Id";

        public static Bush AddBushModData(Bush bush, Object __instance)
        {
            if (ModEntry.customBushApi != null && ModEntry.modHelper.ModRegistry.IsLoaded("furyx639.CustomBush"))
            {
                IEnumerable<(string Id, ICustomBush Data)> customBushData = ModEntry.customBushApi.GetData();
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
