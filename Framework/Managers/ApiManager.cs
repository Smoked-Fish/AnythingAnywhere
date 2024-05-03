using StardewModdingAPI;
using AnythingAnywhere.Framework.Interfaces;
using System.Collections.Generic;
using System;

namespace AnythingAnywhere.Framework.Managers
{
    internal class ApiManager
    {
        private readonly Dictionary<Type, object> _apis;

        public ApiManager() 
        {
            _apis = new Dictionary<Type, object>();
        }

        public T GetApi<T>(string apiName, bool logError = true) where T : class
        {
            if (_apis.TryGetValue(typeof(T), out var api) && api is T typedApi)
            {
                return typedApi;
            }

            api = ModEntry.modHelper.ModRegistry.GetApi<T>(apiName);

            if (api == null)
            {
                if (logError)
                {
                    ModEntry.monitor.Log($"Failed to hook into {apiName}.", LogLevel.Error);
                }
                else 
                {
                    ModEntry.monitor.Log($"Failed to hook into {apiName}.", LogLevel.Trace);
                }
                return null;
            }

            _apis[typeof(T)] = api;
            ModEntry.monitor.Log($"Successfully hooked into {apiName}.", LogLevel.Trace);
            return (T)api;
        }
    }
}
