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
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(GlobalEventManager))]
internal static class ATG
{
    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.ATG.EnableEdit.Value)
        {
            return;
        }

        Mdh.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(FireOrbIfItsFair);
    }


    private static void FireOrbIfItsFair(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        ILLabel skipAtgFireMissile = w.DefineLabel();
        int missileDamageVariableNumber = 0;


        // going to start of line:
        // MissileUtils.FireMissile(characterBody.corePosition, characterBody, damageInfo.procChainMask, victim, num7, damageInfo.crit, GlobalEventManager.CommonAssets.missilePrefab, DamageColorIndex.Item, true);
        w.MatchRelaxed(
            x => x.MatchCallOrCallvirt("RoR2.Util", "OnHitProcDamage"),
            x => x.MatchStloc(out missileDamageVariableNumber),
            x => x.MatchLdloc(0) && w.SetCurrentTo(x),
            x => x.MatchCallOrCallvirt<CharacterBody>("get_corePosition"),
            x => x.MatchLdloc(0),
            x => x.MatchLdarg(1),
            x => x.MatchLdfld(out _)
        ).ThrowIfFailure()
        .InsertBeforeCurrent(
            w.Create(OpCodes.Ldloc, 0), // attackerBody
            w.Create(OpCodes.Ldloc, missileDamageVariableNumber), // missileDamage
            w.Create(OpCodes.Ldarg_1), // DamageInfo
            w.Create(OpCodes.Ldarg_2), // victim
            w.CreateCall(FireMissileOrbReturnFairness),
            w.Create(OpCodes.Brtrue, skipAtgFireMissile)
        );


        // going after firemissile line
        w.MatchRelaxed(
            x => x.MatchLdsfld("RoR2.GlobalEventManager/CommonAssets", "missilePrefab"),
            x => x.MatchLdcI4(3),
            x => x.MatchLdcI4(1),
            x => x.MatchCallOrCallvirt("RoR2.MissileUtils", "FireMissile") && w.SetCurrentTo(x)
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