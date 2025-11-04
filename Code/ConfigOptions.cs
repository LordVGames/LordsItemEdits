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
            public static ConfigEntry<bool> EnableEdit;
            public static ConfigEntry<bool> AllowRespawnAsVoidReaver;
        }

        public static class ATG
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class PocketICBM
        {
            public static ConfigEntry<bool> EnableEdit;
            public static ConfigEntry<bool> ChangeATGEffect;
            public static ConfigEntry<bool> ChangeArmedBackpackEffect;
            public static ConfigEntry<bool> ChangeGenericMissileEffect;
            public static ConfigEntry<bool> ChangePlasmaShrimpEffect;
            public static ConfigEntry<bool> ChangeRocketSurvivorEffect;
            public static ConfigEntry<bool> ChangeRiskyTweaksScrapLauncherEffect;
        }

        public static class BottledChaos
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class Planula
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class ElectricBoomerang
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class MoltenPerforator
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        public static class ExecutiveCard
        {
            public static ConfigEntry<bool> EnableEdit;
            public static ConfigEntry<bool> AddCreditCardToBottledChaos;
        }

        public static class Polylute
        {
            public static ConfigEntry<bool> EnableEdit;
        }

        internal static void BindConfigOptions(ConfigFile config)
        {
            VoidDios.EnableEdit = config.BindOption(
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
                "Enable the ATG edit which makes it fire like plasma shrimp? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            PocketICBM.EnableEdit = config.BindOption(
                "Pocket ICBM",
                "Enable Edit",
                "Enable the Pocket I.C.B.M edit that triples missile damage instead of firing 2 extra missiles? This will help performance! Disabling this will also disable all effect options below.",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeArmedBackpackEffect = config.BindOption(
                "Pocket ICBM",
                "Change Armed Backpack Effect",
                "Make Armed Backpack's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeATGEffect = config.BindOption(
                "Pocket ICBM",
                "Change ATG Effect",
                "Make ATG's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeGenericMissileEffect = config.BindOption(
                "Pocket ICBM",
                "Change Effect for generic missiles",
                "Make DML's and Engineer Harpoons' ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangePlasmaShrimpEffect = config.BindOption(
                "Pocket ICBM",
                "Change Plasma Shrimp Effect",
                "Make Plasma Shrimp's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeRocketSurvivorEffect = config.BindOption(
                "Pocket ICBM",
                "Change The Rocket Survivor Effect",
                "Make Rocket's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            PocketICBM.ChangeRiskyTweaksScrapLauncherEffect = config.BindOption(
                "Pocket ICBM",
                "Change RiskyTweaks MUL-T Scrap Launcher Effect",
                "Make RiskyTweaks' MUL-T scrap launcher ICBM synergy effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
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


            /*Planula.EnableEdit = config.BindOption(
                "Planula",
                "Enable Edit",
                "Enable the Planula edit that makes it only proc on stunning skill hits?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );*/


            ElectricBoomerang.EnableEdit = config.BindOption(
                "Electric Boomerang",
                "Enable Edit",
                "Enable the Electric Boomerang edit that entirely changes what the item does?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            MoltenPerforator.EnableEdit = config.BindOption(
                "Molten Perforator",
                "Enable Edit",
                "Enable the Molten Perforator edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            ExecutiveCard.EnableEdit = config.BindOption(
                "Executive Card",
                "Enable Edit",
                "Enable the Executive Card edit that makes it more like a normal equipment?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ExecutiveCard.AddCreditCardToBottledChaos = config.BindOption(
                "Executive Card",
                "Add edited effect to the Bottled Chaos pool",
                "Should the new effect for Executive Card be added to the equipment effect pool for Bottled Chaos?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            Polylute.EnableEdit = config.BindOption(
                "Polylute",
                "Enable Edit",
                "Enable the Polylute edit that makes it stack damage instead of hit amount?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ArmedBackpack.EnableEdit = config.BindOption(
                "Starstorm 2 - Armed Backpack",
                "Enable edit",
                "Enable the Armed Backpack edit that makes the missile fire like plasma shrimp AND make the ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );


            SS2Items.ErraticGadget.EnableEdit = config.BindOption(
                "Starstorm 2 - Erratic Gadget",
                "Turn double procs into double damage",
                "Make affected lightning procs deal double damage instead of proccing twice? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }
}