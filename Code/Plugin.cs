using System;
using BepInEx;
using HarmonyLib;
using RoR2;

namespace LordsItemEdits
{
    [BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static PluginInfo PluginInfo { get; private set; }
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "LordVGames";
        public const string PluginName = "LordsItemEdits";
        public const string PluginVersion = "1.0.0";

        public void Awake()
        {
            PluginInfo = Info;
            Log.Init(Logger);
            ConfigOptions.BindConfigOptions(Config);

            ItemEdits.VoidDios.Setup();
            ItemEdits.ATG.Setup();
            ItemEdits.Plimp.Setup();

            if (ModSupport.Starstorm2Mod.ModIsRunning)
            {
                ModSupport.Starstorm2.ArmedBackpack.Setup();
                ModSupport.Starstorm2.ErraticGadget.Setup();
            }
        }
    }
}