using StardewModdingAPI;
using AnythingAnywhere.Framework.Interfaces;
using System;

namespace AnythingAnywhere.Framework.Managers
{
    internal class ApiManager
    {
        private IMonitor _monitor;
        private IGenericModConfigMenuApi _genericModConfigMenuApi;

        public ApiManager(IMonitor monitor)
        {
            _monitor = monitor;
        }
        internal bool HookIntoGenericModConfigMenu(IModHelper helper)
        {
            _genericModConfigMenuApi = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (_genericModConfigMenuApi is null)
            {
                _monitor.Log("Failed to hook into spacechase0.GenericModConfigMenu.", LogLevel.Error);
                return false;
            }

            _monitor.Log("Successfully hooked into spacechase0.GenericModConfigMenu.", LogLevel.Debug);
            return true;
        }

        public IGenericModConfigMenuApi GetGenericModConfigMenuApi()
        {
            return _genericModConfigMenuApi;
        }
    }
}
