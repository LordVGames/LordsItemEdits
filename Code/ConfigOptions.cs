using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;

namespace LordsItemEdits
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> EnableVoidDiosEdit;
        public static ConfigEntry<bool> AllowRespawnAsVoidReaver;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableVoidDiosEdit = config.Bind<bool>(
                "Pluripotent Larva",
                "Enable edit", true,
                "Enable the Pluripotent Larva edit?"
            );
            AllowRespawnAsVoidReaver = config.Bind<bool>(
                "Pluripotent Larva",
                "Allow respawning as a Void Reaver", false,
                "Should the Void Reaver be an option to respawn as with the edited Pluripotent Larva? This is configurable since their only attack has bad damage and can't target flying enemies."
            );
        }
    }
}