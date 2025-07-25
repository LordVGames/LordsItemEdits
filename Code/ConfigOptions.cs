using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using MiscFixes.Modules;

namespace LordsItemEdits
{
    public static class ConfigOptions
    {
        public static class SS2Items
        {
            public static class ArmedBackpack
            {
                public static ConfigEntry<bool> EnableEdit;
            }

            public static class ErraticGadget
            {
                public static ConfigEntry<bool> EnableEdit;
            }
        }

        public static class VoidDios
        {
            public static ConfigEntry<bool> EnableVoidDiosEdit;
            public static ConfigEntry<bool> AllowRespawnAsVoidReaver;
        }

        public static class ATG
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class PocketICBM
        {
            public static ConfigEntry<bool> ChangeATGEffect;
            public static ConfigEntry<bool> ChangeArmedBackpackEffect;
            public static ConfigEntry<bool> ChangeDMLEffect;
            public static ConfigEntry<bool> ChangeEngiHarpoonEffect;
            public static ConfigEntry<bool> ChangePlasmaShrimpEffect;
            public static ConfigEntry<bool> ChangeRocketSurvivorEffect;
            public static ConfigEntry<bool> ChangeRiskyTweaksScrapLauncherEffect;
        }

        public static class BottledChaos
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        internal static void BindConfigOptions(ConfigFile config)
        {
            VoidDios.EnableVoidDiosEdit = config.BindOption(
                "Pluripotent Larva",
                "Enable edit",
                "Enable the Pluripotent Larva edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            VoidDios.AllowRespawnAsVoidReaver = config.BindOption(
                "Pluripotent Larva",
                "Allow respawning as a Void Reaver",
                "Should the Void Reaver be an option to respawn as with the edited Pluripotent Larva? This is configurable since their only attack has bad damage and can't target flying enemies.",
                false
            );


            ATG.EnableEdit = config.BindOption(
                "ATG",
                "Enable edit",
                "Enable the ATG edit which makes it fire like plasma shrimp? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            PocketICBM.ChangeArmedBackpackEffect = config.BindOption(
                "Pocket ICBM",
                "Change Armed Backpack Effect",
                "Make Armed Backpack's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeATGEffect = config.BindOption(
                "Pocket ICBM",
                "Change ATG Effect",
                "Make ATG's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeDMLEffect = config.BindOption(
                "Pocket ICBM",
                "Change Disposable Missile Launcher Effect",
                "Make DML's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeEngiHarpoonEffect = config.BindOption(
                "Pocket ICBM",
                "Change Engineer Harpoons Skill Effect",
                "Make Engineer's Harpoons skill's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangePlasmaShrimpEffect = config.BindOption(
                "Pocket ICBM",
                "Change Plasma Shrimp Effect",
                "Make Plasma Shrimp's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeRocketSurvivorEffect = config.BindOption(
                "Pocket ICBM",
                "Change The Rocket Survivor Effect",
                "Make Rocket's ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeRiskyTweaksScrapLauncherEffect = config.BindOption(
                "Pocket ICBM",
                "Change RiskyTweaks MUL-T Scrap Launcher Effect",
                "Make RiskyTweaks' MUL-T scrap launcher ICBM synergy effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            BottledChaos.EnableEdit = config.BindOption(
                "Bottled Chaos",
                "Enable Edit",
                "Enable the Bottled Chaos edit which makes it give some equipment cooldown reduction alongside it's normal effect?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ArmedBackpack.EnableEdit = config.BindOption(
                "Starstorm 2 - Armed Backpack",
                "Enable edit",
                "Enable the Armed Backpack edit that makes the missile fire like plasma shrimp AND make the ICBM effect triple missile damage instead of firing 2 extra missiles? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ErraticGadget.EnableEdit = config.BindOption(
                "Starstorm 2 - Erratic Gadget",
                "Turn double procs into double damage",
                "Make affected lightning procs deal double damage instead of proccing twice? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }
}