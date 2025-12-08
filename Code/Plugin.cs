using System;
using BepInEx;
using MiscFixes.Modules;
using MonoDetour;
namespace LordsItemEdits;


[BepInDependency(SS2.SS2Main.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RocketSurvivor.RocketSurvivorPlugin.MODUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModSupport.RiskyTweaksMod.RiskyTweaksMod.GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModSupport.WolfFixes.WolfFixesMod.ModGUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    public static PluginInfo PluginInfo { get; private set; }
    public const string PluginGUID = PluginAuthor + "." + PluginName;
    public const string PluginAuthor = "LordVGames";
    public const string PluginName = "LordsItemEdits";
    public const string PluginVersion = "0.6.0";

    public void Awake()
    {
        PluginInfo = Info;
        Log.Init(Logger);
        ConfigOptions.BindConfigOptions(Config);
        MonoDetourManager.InvokeHookInitializers(typeof(Plugin).Assembly, reportUnloadableTypes: false);
        ModLanguage.AddNewLangTokens();
    }
}