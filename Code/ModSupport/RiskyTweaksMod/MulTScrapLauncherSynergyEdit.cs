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

namespace LordsItemEdits.ModSupport.RiskyTweaksMod
{
    [MonoDetourTargets(typeof(ScrapICBM), GenerateControlFlowVariants = true)]
    internal class MulTScrapLauncherSynergyEdit
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.PocketICBM.ChangeRiskyTweaksScrapLauncherEffect.Value)
            {
                return;
            }

            var m = DefaultMonoDetourManager.New();
            m.LogFilter = MonoDetourLogger.LogChannel.IL;
            MonoDetourHooks.RiskyTweaks.Tweaks.Survivors.Toolbot.ScrapICBM.FireGrenadeLauncher_ModifyProjectileAimRay.ControlFlowPrefix(OverrideICBMSynergy, manager: m);
            //MonoDetourHooks.RiskyTweaks.Tweaks.Survivors.Toolbot.ScrapICBM.FireGrenadeLauncher_ModifyProjectileAimRay.ILHook(JustDoMyThing);
        }

        private static void JustDoMyThing(ILManipulationInfo info)
        {
            ILWeaver w = new(info);


            w.CurrentTo(0); // just making sure
            w.InsertBeforeCurrent(
                // doing my edit then copying the IL for calling orig
                w.Create(OpCodes.Ldarg_2),
                w.CreateCall(ChangeICBMSynergy),
                w.Create(OpCodes.Ldarg_1),
                w.Create(OpCodes.Ldarg_2),
                w.Create(OpCodes.Ldarg_3),
                w.CreateCall(JustCallOrig),
                w.Create(OpCodes.Stloc, 18),
                w.Create(OpCodes.Ret)
            );
        }

        private static void ChangeICBMSynergy(EntityStates.Toolbot.FireGrenadeLauncher fireGrenadeLauncherState)
        {
            fireGrenadeLauncherState.damageCoefficient *= PocketICBM.GetICBMDamageMult(fireGrenadeLauncherState.characterBody);
        }
        private static void JustCallOrig(On.EntityStates.Toolbot.FireGrenadeLauncher.orig_ModifyProjectileAimRay orig, EntityStates.Toolbot.FireGrenadeLauncher fireGrenadeLauncherState, Ray aimRay)
        {
            orig(fireGrenadeLauncherState, aimRay);
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