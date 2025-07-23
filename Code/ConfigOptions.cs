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
                public static ConfigEntry<bool> EnableArmedBackpackEdit;
            }

            public static class ErraticGadget
            {
                public static ConfigEntry<bool> EnableErraticGadgetEdit;
                public static ConfigEntry<bool> TurnDoubleProcsIntoDoubleDamage;
            }
        }

        public static class VoidDios
        {
            public static ConfigEntry<bool> EnableVoidDiosEdit;
            public static ConfigEntry<bool> AllowRespawnAsVoidReaver;
        }

        public static class ATG
        {
            public static ConfigEntry<bool> EnableAtgEdit;
        }

        public static class PocketICBM
        {
            public static ConfigEntry<bool> EnableICBMEdit;
            public static ConfigEntry<bool> ChangeATGEffect;
            public static ConfigEntry<bool> ChangeArmedBackpackEffect;
            public static ConfigEntry<bool> ChangeDMLEffect;
            public static ConfigEntry<bool> ChangeEngiHarpoonEffect;
            public static ConfigEntry<bool> ChangePlimpEffect;
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


            ATG.EnableAtgEdit = config.BindOption(
                "ATG",
                "Enable edit",
                "Enable the ATG edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            PocketICBM.EnableICBMEdit = config.BindOption(
                "Pocket ICBM",
                "Enable edit",
                "Enable the Pocket ICBM edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeArmedBackpackEffect = config.BindOption(
                "Pocket ICBM",
                "Change Armed Backpack Effect",
                "Make Armed Backpack's ICBM effect triple missile damage instead of firing 2 extra missiles?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeATGEffect = config.BindOption(
                "Pocket ICBM",
                "Change ATG Effect",
                "Make ATG's ICBM effect triple missile damage instead of firing 2 extra missiles?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeDMLEffect = config.BindOption(
                "Pocket ICBM",
                "Change Disposable Missile Launcher Effect",
                "Make DML's ICBM effect triple missile damage instead of firing 2 extra missiles?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeEngiHarpoonEffect = config.BindOption(
                "Pocket ICBM",
                "Change Engineer Harpoons Skill Effect",
                "Make Engineer's Harpoons skill's ICBM effect triple missile damage instead of firing 2 extra missiles?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangePlimpEffect = config.BindOption(
                "Pocket ICBM",
                "Change Plasma Shrimp Effect",
                "Make Plasma Shrimp's ICBM effect triple missile damage instead of firing 2 extra missiles?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ArmedBackpack.EnableArmedBackpackEdit = config.BindOption(
                "Starstorm 2 - Armed Backpack",
                "Enable edit",
                "Enable the Armed Backpack edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ErraticGadget.EnableErraticGadgetEdit = config.BindOption(
                "Starstorm 2 - Erratic Gadget",
                "Enable edit",
                "Enable the Erratic Gadget edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            SS2Items.ErraticGadget.TurnDoubleProcsIntoDoubleDamage = config.BindOption(
                "Starstorm 2 - Erratic Gadget",
                "Turn double procs into double damage",
                "Make affected lightning procs deal double damage instead of proccing twice? Will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }
}