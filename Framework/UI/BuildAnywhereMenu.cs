using Newtonsoft.Json;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Crops;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace AnythingAnywhere.Framework.UI
{
    internal class BuildAnywhereMenu : CarpenterMenu
    {
        // Prevents Better Juminos from spamming errors
        #pragma warning disable CS0649
        public bool magicalConstruction;
        #pragma warning restore CS0649

        public BuildAnywhereMenu(string builder, GameLocation targetLocation = null) : base(builder, targetLocation)
        {
            IDictionary<string, BuildingData> modifiedData = ProcessBuildingData(Game1.buildingData, ModEntry.modConfig.EnableInstantBuild, ModEntry.modConfig.EnableGreenhouse, ModEntry.modConfig.RemoveBuildConditions);

            int num = 0;
            this.Blueprints.Clear();
            foreach (var keyValuePair in modifiedData)
            {
                if (keyValuePair.Value.Builder != builder || !GameStateQuery.CheckConditions(keyValuePair.Value.BuildCondition, targetLocation) || (keyValuePair.Value.BuildingToUpgrade != null && TargetLocation.getNumberBuildingsConstructed(keyValuePair.Value.BuildingToUpgrade) == 0) || !IsValidBuildingForLocation(keyValuePair.Key, keyValuePair.Value, TargetLocation))
                    continue;


                this.Blueprints.Add(new BlueprintEntry(num++, keyValuePair.Key, keyValuePair.Value, null));
                if (keyValuePair.Value.Skins != null)
                {
                    foreach (BuildingSkin skin in keyValuePair.Value.Skins)
                    {
                        if (skin.ShowAsSeparateConstructionEntry && GameStateQuery.CheckConditions(skin.Condition, TargetLocation))
                            this.Blueprints.Add(new BlueprintEntry(num++, keyValuePair.Key, keyValuePair.Value, skin.Id));
                    }
                }
            }
            SetNewActiveBlueprint(0);
        }

        public static IDictionary<string, BuildingData> ProcessBuildingData(IDictionary<string, BuildingData> buildingData, bool enableInstantBuild, bool enableGreenhouse, bool removeBuildConditions)
        {
            var modifiedBuildingData = new Dictionary<string, BuildingData>();

            foreach (var data in buildingData)
            {
                BuildingData modifiedData = ModifyBuildingData(data.Value, enableInstantBuild, enableGreenhouse, removeBuildConditions);
                modifiedBuildingData.Add(data.Key, modifiedData);
            }

            return modifiedBuildingData;
        }

        private static BuildingData ModifyBuildingData(BuildingData originalData, bool enableInstantBuild, bool enableGreenhouse, bool removeBuildConditions)
        {
            BuildingData modifiedData = DeepCopy(originalData);

            if (enableGreenhouse && IsGreenhouse(originalData))
            {
                SetGreenhouseAttributes(modifiedData);
            }

            if (enableInstantBuild)
            {
                SetInstantBuildAttributes(modifiedData);
            }

            if (removeBuildConditions)
            {
                RemoveBuildConditions(modifiedData);
            }

            return modifiedData;
        }

        private static bool IsGreenhouse(BuildingData data)
        {
            return TokenParser.ParseText(data.Name) == Game1.content.LoadString("Strings\\Buildings:Greenhouse_Name");
        }

        private static void SetGreenhouseAttributes(BuildingData data)
        {
            List<BuildingMaterial> greenhouseMaterials = new List<BuildingMaterial>
        {
            new BuildingMaterial { ItemId = "(O)709", Amount = 100 },
            new BuildingMaterial { ItemId = "(O)338", Amount = 20 },
            new BuildingMaterial { ItemId = "(O)337", Amount = 10 },
        };

            data.Builder = "Robin";
            data.BuildCost = 150000;
            data.BuildDays = 3;
            data.BuildMaterials = greenhouseMaterials;
            data.BuildCondition = "PLAYER_HAS_MAIL Host ccPantry";
        }

        private static void SetInstantBuildAttributes(BuildingData data)
        {
            data.MagicalConstruction = true;
            data.BuildCost = 0;
            data.BuildDays = 0;
            data.BuildMaterials = new List<BuildingMaterial>(); // Assuming this clears the list
        }

        private static void RemoveBuildConditions(BuildingData data)
        {
            data.BuildCondition = "";
        }

        private static BuildingData DeepCopy(BuildingData source)
        {
            if (source == null)
                return null;

            string serializedObject = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<BuildingData>(serializedObject);
        }

        public override bool IsValidBuildingForLocation(string typeId, BuildingData data, GameLocation targetLocation)
        {
            if ((typeId == "Cabin" && TargetLocation.Name != "Farm") && !ModEntry.modConfig.EnableCabinsAnywhere)
                return false;

            return true;
        }

    }
}
