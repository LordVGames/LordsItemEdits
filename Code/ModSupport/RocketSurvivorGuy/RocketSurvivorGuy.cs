using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ModSupport.RocketSurvivorGuy
{
    // dumb naming because the rocket plugin uses 2 of the names i would've used for this
    internal static class RocketSurvivorGuy
    {
        private static bool? _enabled;

        internal static bool ModIsRunning
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(RocketSurvivor.RocketSurvivorPlugin.MODUID);
                return (bool)_enabled;
            }
        }
    }
}