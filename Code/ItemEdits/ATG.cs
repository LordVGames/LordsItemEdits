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
using RoR2.Orbs;
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
        FireMissileOrb(attackerBody, missileDamage, damageInfo, victim);
        // we need to return if it's fair or not (aka if we should skip the firemissile line or not)
        return attackerBody.teamComponent.teamIndex == TeamIndex.Player;
    }



    internal static void FireMissileOrb(CharacterBody attackerBody, float missileDamage, DamageInfo damageInfo, GameObject victim)
    {
        if (victim == null || attackerBody.teamComponent.teamIndex != TeamIndex.Player)
        {
            return;
        }
        CharacterBody victimBody = victim.GetComponent<CharacterBody>();
        FireMissileOrb(attackerBody, missileDamage, damageInfo, victimBody, true);
    }

    internal static void FireMissileOrb(CharacterBody attackerBody, float missileDamage, DamageInfo damageInfo, CharacterBody victimBody, bool addMissileProc)
    {
        if (victimBody == null || attackerBody == null || attackerBody.inventory == null || attackerBody.teamComponent.teamIndex != TeamIndex.Player)
        {
            return;
        }


        MicroMissileOrb missileOrb = new()
        {
            origin = attackerBody.aimOrigin,
            damageValue = missileDamage,
            isCrit = damageInfo.crit,
            teamIndex = attackerBody.teamComponent.teamIndex,
            attacker = attackerBody.gameObject,
            procChainMask = damageInfo.procChainMask,
            procCoefficient = 1f,
            damageColorIndex = DamageColorIndex.Item,
            target = victimBody.mainHurtBox
        };
        if (addMissileProc)
        {
            missileOrb.procChainMask.AddProc(ProcType.Missile);
        }


        if (
            ConfigOptions.PocketICBM.EnableEdit.Value &&
            (ConfigOptions.PocketICBM.ChangeATGEffect.Value && addMissileProc || ConfigOptions.PocketICBM.ChangeArmedBackpackEffect.Value && !addMissileProc)
        )
        {
            // already hooked GetMoreMissileDamageMultiplier to do the edited version's damage so it's fine to use here
            missileOrb.damageValue *= MissileUtils.GetMoreMissileDamageMultiplier(attackerBody.inventory.GetItemCountEffective(DLC1Content.Items.MissileVoid));
        }
        else
        {
            OrbManager.instance.AddOrb(missileOrb);
            OrbManager.instance.AddOrb(missileOrb);
            // gotta be authentic with the missile spam experience lmao
            Util.PlaySound("Play_item_proc_missile_fire", attackerBody.gameObject);
            Util.PlaySound("Play_item_proc_missile_fire", attackerBody.gameObject);
        }
        OrbManager.instance.AddOrb(missileOrb);
        // the orb doesn't play a sound on fire and editing the assets isn't working so
        Util.PlaySound("Play_item_proc_missile_fire", attackerBody.gameObject);
    }
}