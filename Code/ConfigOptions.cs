using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using MiscFixes.Modules;

namespace LordsItemEdits
{
    public static class ConfigOptions
    {
        public static ConfigEntry<bool> EnableVoidDiosEdit;
        public static ConfigEntry<bool> AllowRespawnAsVoidReaver;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableVoidDiosEdit = config.BindOption(
                "Pluripotent Larva",
                "Enable edit",
                "Enable the Pluripotent Larva edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            AllowRespawnAsVoidReaver = config.BindOption(
                "Pluripotent Larva",
                "Allow respawning as a Void Reaver",
                "Should the Void Reaver be an option to respawn as with the edited Pluripotent Larva? This is configurable since their only attack has bad damage and can't target flying enemies.",
                false
            );
        }
    }
}