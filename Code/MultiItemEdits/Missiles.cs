using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using UnityEngine;
using RoR2;
using RoR2.Orbs;
using LordsItemEdits.ItemEdits;
namespace LordsItemEdits.MultiItemEdits;


[MonoDetourTargets(typeof(MissileUtils))]
internal static class Missiles
{
    [MonoDetourHookInitialize]
    private static void Setup()
    {
        // this affects both engi harpoon and DML this is in here
        if (ConfigOptions.PocketICBM.EnableEdit.Value && ConfigOptions.PocketICBM.ChangeGenericMissileEffect.Value)
        {
            Mdh.RoR2.MissileUtils.FireMissile_UnityEngine_Vector3_RoR2_CharacterBody_RoR2_ProcChainMask_UnityEngine_GameObject_System_Single_System_Boolean_UnityEngine_GameObject_RoR2_DamageColorIndex_UnityEngine_Vector3_System_Single_System_Boolean.ILHook(ChangeICBMEffect);
        }
    }

    private static void ChangeICBMEffect(ILManipulationInfo info)
    {
        ILWeaver w = new(info);


        // near the end of line:
        // float num2 = Mathf.Max(1f, 1f + 0.5f * (float)(num - 1));
        w.MatchRelaxed(
            x => x.MatchMul(),
            x => x.MatchAdd(),
            x => x.MatchCallOrCallvirt<Mathf>("Max") && w.SetCurrentTo(x),
            x => x.MatchStloc(1)
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.Create(OpCodes.Ldarg_1), // CharacterBody
            w.CreateCall(ReplaceOldICBMDamageMult)
        );


        // jumping to this line:
        // if (num > 0)
        // which is after this line:
        // ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
        ILLabel jumpToEnd = w.DefineLabel();
        w.MatchRelaxed(
            x => x.MatchLdloc(0) && w.SetCurrentTo(x),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out jumpToEnd),
            x => x.MatchLdloc(2),
            x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Implicit")
        ).ThrowIfFailure()
        .InsertBeforeCurrent(
            w.Create(OpCodes.Br, jumpToEnd)
        );
    }

    private static float ReplaceOldICBMDamageMult(float oldDamageMult, CharacterBody attackerBody)
    {
        return PocketICBM.GetICBMDamageMult(attackerBody);
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
        if (victimBody == null || attackerBody.teamComponent.teamIndex != TeamIndex.Player)
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
            missileOrb.damageValue *= PocketICBM.GetICBMDamageMult(attackerBody);
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