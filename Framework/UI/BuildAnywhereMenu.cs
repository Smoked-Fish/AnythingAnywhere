using StardewValley;
using StardewModdingAPI;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnythingAnywhere.Framework.UI
{
    internal class BuildAnywhereMenu : CarpenterMenu
    {
        public BuildAnywhereMenu(string builder, ModConfig config, IMonitor monitor) : base(builder, Game1.currentLocation)
        {
            TargetLocation = Game1.currentLocation;
            int index = 0;
            Blueprints.Clear();
            foreach (KeyValuePair<string, BuildingData> data in Game1.buildingData)
            {

                if ((data.Value.Builder != builder || !GameStateQuery.CheckConditions(data.Value.BuildCondition, TargetLocation) || data.Value.BuildingToUpgrade != null && TargetLocation.getNumberBuildingsConstructed(data.Value.BuildingToUpgrade) == 0 || !IsValidBuildingForLocation(data.Key, data.Value, TargetLocation)) && !config.EnableInstantBuild)
                {
                    continue;
                }
                else if (config.EnableInstantBuild)
                {
                    if (data.Value.Builder != builder)
                        continue;
                    BuildingData copyData = DeepCopy(data.Value);

                    copyData.MagicalConstruction = true;
                    copyData.BuildCost = 0;
                    copyData.BuildDays = 0;
                    copyData.BuildMaterials = [];

                    Blueprints.Add(new BlueprintEntry(index++, data.Key, copyData, null));
                }
                else
                {
                    Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, null));
                }
                /*monitor.LogOnce($"Blueprint Added. Index: {index}\nData: {data.Key}\nData: {data.Value}", LogLevel.Info);*/
                if (data.Value.Skins == null)
                {
                    continue;
                }
                foreach (BuildingSkin skin in data.Value.Skins)
                {
                    if (skin.ShowAsSeparateConstructionEntry && GameStateQuery.CheckConditions(skin.Condition, TargetLocation))
                    {
                        Blueprints.Add(new BlueprintEntry(index++, data.Key, data.Value, skin.Id));
                    }
                }
            }
            SetNewActiveBlueprint(0);
        }

        private BuildingData DeepCopy(BuildingData source)
        {
            if (source == null)
                return null;

            string serializedObject = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<BuildingData>(serializedObject);
        }
    }
}
