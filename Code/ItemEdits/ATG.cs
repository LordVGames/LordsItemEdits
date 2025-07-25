using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using RoR2;
using LordsItemEdits.MultiItemEdits;

namespace LordsItemEdits.ItemEdits
{
    internal class ATG
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.ATG.EnableEdit.Value)
            {
                return;
            }

            MonoDetourHooks.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(FireOrbIfFair);
        }


        private static void FireOrbIfFair(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipAtgFireMissile = w.DefineLabel();


            // going before "MissileUtils.FireMissile(characterBody.corePosition, characterBody, damageInfo.procChainMask, victim, num7, damageInfo.crit, GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);"
            w.MatchRelaxed(
                x => x.MatchLdloc(0) && w.SetCurrentTo(x),
                x => x.MatchCallOrCallvirt(out _),
                x => x.MatchLdloc(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld(out _)
            ).ThrowIfFailure()
            .InsertBeforeCurrent(
                w.Create(OpCodes.Ldloc, 0),
                w.Create(OpCodes.Ldloc, 45),
                w.Create(OpCodes.Ldarg_1),
                w.Create(OpCodes.Ldarg_2),
                w.CreateCall(FireMissileOrbReturnFairness),
                w.Create(OpCodes.Brtrue, skipAtgFireMissile)
            );


            // going after firemissile line
            w.MatchRelaxed(
                x => x.MatchLdsfld(out _),
                x => x.MatchLdcI4(3),
                x => x.MatchLdcI4(1),
                x => x.MatchCall(out _) && w.SetCurrentTo(x)
            ).ThrowIfFailure()
            .MarkLabelToCurrentNext(skipAtgFireMissile);
        }
        private static bool FireMissileOrbReturnFairness(CharacterBody attackerBody, float missileDamage, DamageInfo damageInfo, GameObject victim)
        {
            // orb won't fire if it's unfair
            Missiles.FireMissileOrb(attackerBody, missileDamage, damageInfo, victim);
            // we need to return if it's fair or not (aka if we should skip the firemissile line or not)
            return attackerBody.teamComponent.teamIndex == TeamIndex.Player;
        }
    }
}