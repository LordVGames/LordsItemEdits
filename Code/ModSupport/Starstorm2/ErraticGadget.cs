﻿using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Orbs;
using SS2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LordsItemEdits.ModSupport.Starstorm2
{
    [MonoDetourTargets(typeof(SS2.Items.ErraticGadget), GenerateControlFlowVariants = true)]
    internal static class ErraticGadget
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            // this is apparently fixed in the official beta
            if (ConfigOptions.SS2Items.ErraticGadget.EnableProcChainingFix.Value && !ConfigOptions.SS2Items.HookForBetaVersion.Value)
            {
                MonoDetourHooks.SS2.Items.ErraticGadget.Behavior.OnDamageDealtServer.ILHook(FixProcChainingWithSelf);
            }
            if (ConfigOptions.SS2Items.ErraticGadget.EnableEdit.Value)
            {
                LanguageAPI.AddOverlayPath(ModUtil.GetLangFileLocation("ErraticGadget"));
                MonoDetourHooks.SS2.Items.ErraticGadget.LightningOrb_OnArrival.ILHook(SkipDoublingProc);
                MonoDetourHooks.SS2.Items.ErraticGadget.LightningStrikeOrb_OnArrival.ControlFlowPrefix(LightningStrikeOrb_JustDoubleDamage);
                MonoDetourHooks.SS2.Items.ErraticGadget.SimpleLightningStrikeOrb_OnArrival.ControlFlowPrefix(SimpleLightningStrikeOrb_JustDoubleDamage);
            }
        }




        private static void FixProcChainingWithSelf(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipStupid = w.DefineLabel();


            // going in front of line:
            // gadgetLightningOrb.procChainMask = default(ProcChainMask);
            w.MatchRelaxed(
                x => x.MatchLdloc(1) && w.SetCurrentTo(x),
                x => x.MatchLdflda(out _),
                x => x.MatchInitobj(out _)
            ).ThrowIfFailure()
            .InsertBeforeCurrent(
                w.Create(OpCodes.Br, skipStupid)
            )


            // going past line above
            .CurrentToNext()
            .CurrentToNext()
            .InsertAfterCurrent(w.Create(OpCodes.Ldarg_1))
            .MarkLabelToCurrent(skipStupid)
            .InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_1),
                w.CreateCall(ActuallySetProcChainMask)
            );
        }
        private static void ActuallySetProcChainMask(DamageReport damageReport, SS2.Items.ErraticGadget.GadgetLightningOrb gadgetLightningOrb)
        {
            gadgetLightningOrb.procChainMask = damageReport.damageInfo.procChainMask;
        }


        private static void SkipDoublingProc(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipDoublingProc = w.DefineLabel();


            if (ConfigOptions.SS2Items.HookForBetaVersion.Value)
            {
                // next 2 blocks are to skip over the part where lightning is doubled
                // going to before "bool flag2 = false;"
                w.MatchRelaxed(
                    x => x.MatchLdcI4(0) && w.SetCurrentTo(x),
                    x => x.MatchStloc(0)
                ).ThrowIfFailure();
                w.InsertBeforeCurrentStealLabels(
                    w.Create(OpCodes.Br, skipDoublingProc)
                );


                // going to before:
                // orig.Invoke(self);
                // by matching after this line:
                // self.bouncedObjects.RemoveAt(self.bouncedObjects.Count - 1);
                w.MatchRelaxed(
                    x => x.MatchLdcI4(1),
                    x => x.MatchSub(),
                    x => x.MatchCallvirt(out _) && w.SetCurrentTo(x),
                    x => x.MatchNop(),
                    x => x.MatchNop(),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdarg(2)
                ).ThrowIfFailure();
                w.InsertAfterCurrent(w.Create(OpCodes.Ldarg_2));
                w.MarkLabelToCurrent(skipDoublingProc);
                w.InsertAfterCurrent(
                    w.CreateCall(DealDoubleDamage)
                );
            }
            else
            {
                // next 2 blocks are to skip over the part where lightning is doubled
                // going to before "bool flag = false;"
                w.MatchRelaxed(
                    x => x.MatchLdcI4(0) && w.SetCurrentTo(x),
                    x => x.MatchStloc(0)
                ).ThrowIfFailure();
                w.InsertBeforeCurrentStealLabels(
                    w.Create(OpCodes.Br, skipDoublingProc)
                );


                // going to before:
                // orig.Invoke(self);
                // by matching after this line:
                // self.bouncedObjects.RemoveAt(self.bouncedObjects.Count - 1);
                w.MatchRelaxed(
                    x => x.MatchLdcI4(1),
                    x => x.MatchSub(),
                    x => x.MatchCallvirt(out _) && w.SetCurrentTo(x),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdarg(2)
                ).ThrowIfFailure();
                w.InsertAfterCurrent(w.Create(OpCodes.Ldarg_2));
                w.MarkLabelToCurrent(skipDoublingProc);
                w.InsertAfterCurrent(
                    w.CreateCall(DealDoubleDamage)
                );
            }
            //w.LogILInstructions();
        }
        private static void DealDoubleDamage(LightningOrb lightningOrb)
        {
            if (!AllowErraticGadgetDamageBuff(lightningOrb.attacker))
            {
                return;
            }

            lightningOrb.damageValue *= 2;
        }


        // charged perforator
        private static ReturnFlow SimpleLightningStrikeOrb_JustDoubleDamage(SS2.Items.ErraticGadget self, ref On.RoR2.Orbs.SimpleLightningStrikeOrb.orig_OnArrival orig, ref RoR2.Orbs.SimpleLightningStrikeOrb simpleLightningStrikeOrb)
        {
            if (!AllowErraticGadgetDamageBuff(simpleLightningStrikeOrb.attacker))
            {
                orig(simpleLightningStrikeOrb);
                return ReturnFlow.SkipOriginal;
            }

            simpleLightningStrikeOrb.damageValue *= 2;

            orig(simpleLightningStrikeOrb);
            return ReturnFlow.SkipOriginal;
        }


        // royal capacitor
        private static ReturnFlow LightningStrikeOrb_JustDoubleDamage(SS2.Items.ErraticGadget self, ref On.RoR2.Orbs.LightningStrikeOrb.orig_OnArrival orig, ref RoR2.Orbs.LightningStrikeOrb lightningStrikeOrb)
        {
            if (!AllowErraticGadgetDamageBuff(lightningStrikeOrb.attacker))
            {
                orig(lightningStrikeOrb);
                return ReturnFlow.SkipOriginal;
            }

            lightningStrikeOrb.damageValue *= 2;

            orig(lightningStrikeOrb);
            return ReturnFlow.SkipOriginal;
        }




        private static bool AllowErraticGadgetDamageBuff(GameObject attacker)
        {
            if (attacker == null)
            {
                return false;
            }
            var attackerBody = attacker.GetComponent<CharacterBody>();
            if (attackerBody == null || attackerBody.inventory == null)
            {
                return false;
            }
            if (attackerBody.inventory.GetItemCount(SS2Content.Items.ErraticGadget) < 1)
            {
                return false;
            }
            return true;
        }
    }
}