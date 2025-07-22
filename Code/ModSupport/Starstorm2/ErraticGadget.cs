using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.DetourTypes;
using MonoDetour.Cil;
using UnityEngine;
using RoR2;
using SS2;

namespace LordsItemEdits.ModSupport.Starstorm2
{
    [MonoDetourTargets(typeof(SS2.Items.ErraticGadget), GenerateControlFlowVariants = true)]
    internal static class ErraticGadget
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            On.SS2.Items.ErraticGadget.LightningStrikeOrb_OnArrival.ControlFlowPrefix(SS2_LightningStrikeOrb_OnArrival);
        }



        private static ReturnFlow SS2_LightningStrikeOrb_OnArrival(SS2.Items.ErraticGadget self, ref On.RoR2.Orbs.LightningStrikeOrb.orig_OnArrival orig, ref RoR2.Orbs.LightningStrikeOrb lightningStrikeOrb)
        {
            if (!AllowErraticGadgetBuff(lightningStrikeOrb.attacker))
            {
                orig(lightningStrikeOrb);
                return ReturnFlow.SkipOriginal;
            }

            lightningStrikeOrb.damageValue *= 2;

            orig(lightningStrikeOrb);
            return ReturnFlow.SkipOriginal;
        }



        private static bool AllowErraticGadgetBuff(GameObject attacker)
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

        private static void TEST()
        {
            Log.Warning("TEST");
        }
    }
}