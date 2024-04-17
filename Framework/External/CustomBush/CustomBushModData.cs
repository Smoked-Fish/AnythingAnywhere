using AnythingAnywhere.Framework.Interfaces;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace AnythingAnywhere.Framework.External.CustomBush
{
    public class CustomBushModData
    {
        private static string ModID = "furyx639.CustomBush";
        private static string modDataId = ModID + "/Id";

        public static Bush AddBushModData(Bush bush, Object __instance)
        {
            if (!ModEntry.modHelper.ModRegistry.IsLoaded("furyx639.CustomBush"))
                return bush;

            IEnumerable<(string Id, ICustomBush Data)> customBushData = ModEntry.customBushApi.GetData();
            if (!customBushData.Any(item => item.Id == __instance.QualifiedItemId))
                return bush;


            bush.modData[modDataId] = __instance.QualifiedItemId;
            bush.setUpSourceRect();
            return bush;
        }
    }
}
