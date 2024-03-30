using StardewModdingAPI;
using System;

namespace AnythingAnywhere.ModInteractions
{
    internal class GenericModConfigMenu(IModRegistry modRegistry, IManifest manifest, IMonitor monitor, Func<ModConfig> config, Action reset, Action save)
    {
        public void Register()
        {
            IGenericModConfigMenuApi configMenu = modRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null)
                return;

            configMenu.Register(mod: manifest, reset: reset, save: save);


            // Furniture

            configMenu.AddSectionTitle(
                mod: manifest,
                text: I18n.Config_AnythingAnywhere_Furniture_Title
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableFurniture_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableFurniture_Description,
                getValue: () => config().EnableFurniture,
                setValue: value => config().EnableFurniture = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableWallFurnitureIndoors_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableWallFurnitureIndoors_Description,
                getValue: () => config().EnableWallFurnitureIndoors,
                setValue: value => config().EnableWallFurnitureIndoors = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableRugTweaks_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableRugTweaks_Description,
                getValue: () => config().EnableRugTweaks,
                setValue: value => config().EnableRugTweaks = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableFreePlace_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableFreePlace_Description,
                getValue: () => config().EnableFreePlace,
                setValue: value => config().EnableFreePlace = value
            );

            // Buildings

            configMenu.AddSectionTitle(
                mod: manifest,
                text: I18n.Config_AnythingAnywhere_Building_Title
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableBuilding_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableBuilding_Description,
                getValue: () => config().EnableBuilding,
                setValue: value => config().EnableBuilding = value
            );

            configMenu.AddKeybindList(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_BuildMenu_Name,
                tooltip: I18n.Config_AnythingAnywhere_BuildMenu_Description,
                getValue: () => config().BuildMenu,
                setValue: value => config().BuildMenu = value
            );

            configMenu.AddKeybindList(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_WizardBuildMenu_Name,
                tooltip: I18n.Config_AnythingAnywhere_WizardBuildMenu_Description,
                getValue: () => config().WizardBuildMenu,
                setValue: value => config().WizardBuildMenu = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableFreeBuild_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableFreeBuild_Description,
                getValue: () => config().EnableFreeBuild,
                setValue: value => config().EnableFreeBuild = value
            );


            // Other

            configMenu.AddSectionTitle(
                mod: manifest,
                text: I18n.Config_AnythingAnywhere_Other_Title
            );

            configMenu.AddKeybindList(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_TableTweakKeybind_Name,
                tooltip: I18n.Config_AnythingAnywhere_TableTweakKeybind_Description,
                getValue: () => config().TableTweakBind,
                setValue: value => config().TableTweakBind = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableTableTweak_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableTableTweak_Description,
                getValue: () => config().EnableTableTweak,
                setValue: value => config().EnableTableTweak = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_EnableMiniObilisk_Name,
                tooltip: I18n.Config_AnythingAnywhere_EnableMiniObilisk_Name,
                getValue: () => config().AllowMiniObelisksAnywhere,
                setValue: value => config().AllowMiniObelisksAnywhere = value
            );

            configMenu.AddBoolOption(
                mod: manifest,
                name: I18n.Config_AnythingAnywhere_UseJukeboxAnywhere_Name,
                tooltip: I18n.Config_AnythingAnywhere_UseJukeboxAnywhere_Description,
                getValue: () => config().EnableJukeboxFunctionality,
                setValue: value => config().EnableJukeboxFunctionality = value
            );

        }
    }
}
