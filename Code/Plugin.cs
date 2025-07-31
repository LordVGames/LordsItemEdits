using System;
using BepInEx;
using MiscFixes.Modules;

namespace LordsItemEdits
{
    [BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(RocketSurvivor.RocketSurvivorPlugin.MODUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ModSupport.RiskyTweaksMod.RiskyTweaksMod.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "LordsItemEdits";
        public const string PluginVersion = "0.2.0";

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            ConfigOptions.BindConfigOptions(Config);
            Config.WipeConfig();

            MultiItemEdits.Missiles.MonoDetourEdits.Setup();
            ItemEdits.VoidDios.Setup();
            ItemEdits.ATG.Setup();
            ItemEdits.Plimp.Setup();
            ItemEdits.BottledChaos.Setup();
            ItemEdits.MoltenPerforator.Setup();
            //ItemEdits.Planula.Setup();
            ItemEdits.ExecutiveCard.Setup();
            ItemEdits.PocketICBM.Setup();

            if (ModSupport.Starstorm2.Starstorm2Mod.ModIsRunning)
            {
                ModSupport.Starstorm2.ArmedBackpack.Setup();
                ModSupport.Starstorm2.ErraticGadget.Setup();
            }

            if (ModSupport.RocketSurvivorGuy.RocketSurvivorGuy.ModIsRunning)
            {
                ModSupport.RocketSurvivorGuy.PrimaryICBMSupport.Setup();
            }

            if (ModSupport.RiskyTweaksMod.RiskyTweaksMod.ModIsRunning)
            {
                ModSupport.RiskyTweaksMod.MulTScrapLauncherSynergyEdit.Setup();
            }
        }
    }
}