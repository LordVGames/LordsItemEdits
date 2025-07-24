using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ModSupport.Starstorm2
{
    internal class Starstorm2Mod
    {
        private static bool? _enabled;

        internal static bool ModIsRunning
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(SS2.SS2Main.GUID);
                return (bool)_enabled;
            }
        }
    }
}