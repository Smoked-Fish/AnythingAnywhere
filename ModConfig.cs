using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Reflection;
using System;
using System.ComponentModel;

namespace AnythingAnywhere
{
    internal class ModConfig
    {

        // PLACING

        [DefaultValue(true, "Placing")]
        public bool EnablePlacing { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableFreePlace { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableRugRemovalBypass { get; set; }

        [DefaultValue(false, "Placing")]
        public bool EnableWallFurnitureIndoors { get; set; }


        // BUILDING

        [DefaultValue(true, "Building")]
        public bool EnableBuilding { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableBuildAnywhere { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableInstantBuild { get; set; }

        [DefaultValue(SButton.OemComma, "Building")]
        public KeybindList BuildMenu { get; set; }

        [DefaultValue(SButton.OemPeriod, "Building")]
        public KeybindList WizardBuildMenu { get; set; }

        [DefaultValue(SButton.LeftShift, "Building")]
        public KeybindList BuildModifier { get; set; }

        [DefaultValue(true, "Building")]
        public bool EnableGreenhouse { get; set; }

        [DefaultValue(false, "Building")]
        public bool RemoveBuildConditions { get; set; }

        [DefaultValue(false, "Building")]
        public bool EnableBuildingIndoors { get; set; }

        [DefaultValue(false, "Building")]
        public bool BypassMagicInk { get; set; }


        // FARMING
        [DefaultValue(true, "Farming")]
        public bool EnablePlanting { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableDiggingAll { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableFruitTreeTweaks { get; set; }

        [DefaultValue(false, "Farming")]
        public bool EnableWildTreeTweaks { get; set; }



        // OTHER
        [DefaultValue(true, "Other")]
        public bool EnableAnimalRelocate { get; set; }

        [DefaultValue(false, "Other")]
        public bool EnableCaskFunctionality { get; set; }

        [DefaultValue(false, "Other")]
        public bool EnableJukeboxFunctionality { get; set; }

        [DefaultValue(true, "Other")]
        public bool EnableGoldClockAnywhere { get; set; }

        [DefaultValue(false, "Other")]
        public bool MultipleMiniObelisks { get; set; }

        [DefaultValue(false, "Other")]
        public bool EnableCabinsAnywhere { get; set; }

        public ModConfig()
        {
            InitializeDefaultConfig();
        }

        public void InitializeDefaultConfig(string category = null)
        {
            PropertyInfo[] properties = GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                DefaultValueAttribute defaultValueAttribute = (DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute));
                if (defaultValueAttribute != null)
                {
                    object defaultValue = defaultValueAttribute.Value;

                    if (property.PropertyType == typeof(KeybindList) && defaultValue is SButton)
                    {
                        defaultValue = new KeybindList((SButton)defaultValue);
                    }

                    if (category != null && defaultValueAttribute.Category != category)
                    {
                        continue;
                    }

                    property.SetValue(this, defaultValue);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class DefaultValueAttribute : Attribute
    {
        public object Value { get; }
        public string Category { get; }

        public DefaultValueAttribute(object value, string category = null)
        {
            Value = value;
            Category = category;
        }
    }
}
