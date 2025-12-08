using System;
using BepInEx;
using MiscFixes.Modules;
using MonoDetour;
namespace LordsItemEdits;


[BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RocketSurvivor.RocketSurvivorPlugin.MODUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModSupport.RiskyTweaksMod.RiskyTweaksMod.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModSupport.WolfFixes.WolfFixesMod.ModGUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public static PluginInfo PluginInfo { get; private set; }
    public void Awake()
    {
        PluginInfo = Info;
        Log.Init(Logger);
        ConfigOptions.BindAllConfigOptions(Config);
        MonoDetourManager.InvokeHookInitializers(typeof(Plugin).Assembly, reportUnloadableTypes: false);
        ModLanguage.AddNewLangTokens();
    }
}