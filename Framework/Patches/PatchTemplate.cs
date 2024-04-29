using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;


namespace AnythingAnywhere.Framework.Patches
{
    internal class PatchTemplate
    {
        internal static Harmony _harmony;
        internal static Type _object;
        
        internal PatchTemplate(Harmony modHarmony, Type objectType)
        {
            _harmony = modHarmony;
            _object = objectType;
        }

        /// <summary>
        /// Applies method patches using Harmony for a specified target method.
        /// </summary>
        /// <param name="isPostfix">0 for postfix, 1 for prefix.</param>
        /// <param name="originalMethod">The name of the original method to patch.</param>
        /// <param name="newMethod">The name of the method to be applied as a patch.</param>
        /// <param name="parameters">Optional parameters for the method.</param>
        public void Patch(bool isPostfix, string originalMethod, string newMethod, Type[] parameters = null)
        {
            if (isPostfix == true)
                _harmony.Patch(AccessTools.Method(_object, originalMethod, parameters), postfix: new HarmonyMethod(GetType(), newMethod));
            else if (isPostfix == false)
                _harmony.Patch(AccessTools.Method(_object, originalMethod, parameters), prefix: new HarmonyMethod(GetType(), newMethod));
        }
    }
}
