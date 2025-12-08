using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using MiscFixes.Modules;
namespace LordsItemEdits;


public static class ConfigOptions
{
    public static class VoidDios
    {
        public static ConfigEntry<bool> EnableEdit;
        public static ConfigEntry<bool> AllowRespawnAsVoidReaver;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
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

    public static class ATG
    {
        public static ConfigEntry<bool> EnableEdit;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "ATG",
                "Enable edit",
                "Enable the ATG edit which makes it fire like plasma shrimp? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
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

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Pocket ICBM",
                "Enable Edit",
                "Enable the Pocket I.C.B.M edit that triples missile damage instead of firing 2 extra missiles? This will help performance! Disabling this will also disable all effect options below.",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangeArmedBackpackEffect = config.BindOption(
                "Pocket ICBM",
                "Change Armed Backpack Effect",
                "Make Armed Backpack's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangeATGEffect = config.BindOption(
                "Pocket ICBM",
                "Change ATG Effect",
                "Make ATG's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangeGenericMissileEffect = config.BindOption(
                "Pocket ICBM",
                "Change Effect for generic missiles",
                "Make DML's and Engineer Harpoons' ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangePlasmaShrimpEffect = config.BindOption(
                "Pocket ICBM",
                "Change Plasma Shrimp Effect",
                "Make Plasma Shrimp's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangeRocketSurvivorEffect = config.BindOption(
                "Pocket ICBM",
                "Change The Rocket Survivor Effect",
                "Make Rocket's ICBM effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            ChangeRiskyTweaksScrapLauncherEffect = config.BindOption(
                "Pocket ICBM",
                "Change RiskyTweaks MUL-T Scrap Launcher Effect",
                "Make RiskyTweaks' MUL-T scrap launcher ICBM synergy effect triple missile damage instead of firing 2 extra missiles? This will help performance!",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    public static class BottledChaos
    {
        public static ConfigEntry<bool> EnableEdit;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Bottled Chaos",
                "Enable Edit",
                "Enable the Bottled Chaos edit which makes it give some equipment cooldown reduction alongside it's normal effect?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    public static class Planula
    {
        public static ConfigEntry<bool> EnableEdit;
    }

    public static class ElectricBoomerang
    {
        public static ConfigEntry<bool> EnableEdit;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Electric Boomerang",
                "Enable Edit",
                "Enable the Electric Boomerang edit that entirely changes what the item does?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    public static class MoltenPerforator
    {
        public static ConfigEntry<bool> EnableEdit;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Molten Perforator",
                "Enable Edit",
                "Enable the Molten Perforator edit?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    public static class ExecutiveCard
    {
        public static ConfigEntry<bool> EnableEdit;
        public static ConfigEntry<bool> AddCreditCardToBottledChaos;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Executive Card",
                "Enable Edit",
                "Enable the Executive Card edit that makes it more like a normal equipment?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
            AddCreditCardToBottledChaos = config.BindOption(
                "Executive Card",
                "Add edited effect to the Bottled Chaos pool",
                "Should the new effect for Executive Card be added to the equipment effect pool for Bottled Chaos?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    public static class Polylute
    {
        public static ConfigEntry<bool> EnableEdit;

        internal static void BindConfigOptions(ConfigFile config)
        {
            EnableEdit = config.BindOption(
                "Polylute",
                "Enable Edit",
                "Enable the Polylute edit that makes it stack damage instead of hit amount?",
                true,
                Extensions.ConfigFlags.RestartRequired
            );
        }
    }

    internal static void BindAllConfigOptions(ConfigFile config)
    {
        ATG.BindConfigOptions(config);
        BottledChaos.BindConfigOptions(config);
        ElectricBoomerang.BindConfigOptions(config);
        ExecutiveCard.BindConfigOptions(config);
        MoltenPerforator.BindConfigOptions(config);
        PocketICBM.BindConfigOptions(config);
        Polylute.BindConfigOptions(config);
        VoidDios.BindConfigOptions(config);
        config.WipeConfig();
    }
}