using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ModSupport.WolfFixes
{
    internal class WolfFixesMod
    {
        internal const string ModGUID = "Early.Wolfo.WolfFixes";
        private static bool? _enabled;

        internal static bool ModIsRunning
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModGUID);
                return (bool)_enabled;
            }
        }
    }
}
