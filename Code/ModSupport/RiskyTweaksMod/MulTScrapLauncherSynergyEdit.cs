using LordsItemEdits.ItemEdits;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoDetour.Logging;
using MonoMod.Cil;
using RiskyTweaks.Tweaks.Survivors.Toolbot;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;

namespace LordsItemEdits.ModSupport.RiskyTweaksMod
{
    /*[HarmonyPatch]
    internal static class MulTScrapLauncherSynergyEditHarmony
    {
        [HarmonyPatch(typeof(ScrapICBM), nameof(ScrapICBM.FireGrenadeLauncher_ModifyProjectileAimRay))]
        [HarmonyPrefix]
        internal static bool PatchItPls(On.EntityStates.Toolbot.FireGrenadeLauncher.orig_ModifyProjectileAimRay orig, ref EntityStates.Toolbot.FireGrenadeLauncher self, ref Ray aimRay, ref Ray __result)
        {
            self.damageCoefficient *= PocketICBM.GetICBMDamageMult(self.characterBody);
            //orig(self, aimRay);
            return false;
        }
    }*/

    [MonoDetourTargets(typeof(ScrapICBM), GenerateControlFlowVariants = true)]
    internal static class MulTScrapLauncherSynergyEdit
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.PocketICBM.ChangeRiskyTweaksScrapLauncherEffect.Value)
            {
                return;
            }

            //var m = DefaultMonoDetourManager.New();
            //m.LogFilter = MonoDetourLogger.LogChannel.IL;
            //MonoDetourHooks.RiskyTweaks.Tweaks.Survivors.Toolbot.ScrapICBM.FireGrenadeLauncher_ModifyProjectileAimRay.ControlFlowPrefix(OverrideICBMSynergy, manager: m);
            MonoDetourHooks.RiskyTweaks.Tweaks.Survivors.Toolbot.ScrapICBM.FireGrenadeLauncher_ModifyProjectileAimRay.ILHook(JustDoMyThing);
        }

        private static void JustDoMyThing(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipToOrig = w.DefineLabel();
            //w.LogILInstructions();


            w.InsertBeforeCurrent(
                w.Create(OpCodes.Ldarg_2),
                w.CreateCall(ChangeICBMSynergy),
                w.Create(OpCodes.Br, skipToOrig)
            );


            w.MatchRelaxed(
                x => x.MatchLdarg(1) && w.SetCurrentTo(x),
                x => x.MatchLdarg(2),
                x => x.MatchLdarg(3),
                x => x.MatchCallvirt(out _)
            ).ThrowIfFailure()
            .MarkLabelToCurrentPrevious(skipToOrig);
        }
        private static void ChangeICBMSynergy(EntityStates.Toolbot.FireGrenadeLauncher fireGrenadeLauncherState)
        {
            fireGrenadeLauncherState.damageCoefficient *= PocketICBM.GetICBMDamageMult(fireGrenadeLauncherState.characterBody);
        }


        private static ReturnFlow OverrideICBMSynergy(ScrapICBM self, ref On.EntityStates.Toolbot.FireGrenadeLauncher.orig_ModifyProjectileAimRay orig, ref EntityStates.Toolbot.FireGrenadeLauncher fireGrenadeLauncherState, ref Ray aimRay, ref Ray returnValue)
        {
            fireGrenadeLauncherState.damageCoefficient *= PocketICBM.GetICBMDamageMult(fireGrenadeLauncherState.characterBody);
            //returnValue = aimRay;
            orig(fireGrenadeLauncherState, aimRay);
            return ReturnFlow.SkipOriginal;
        }
    }
}