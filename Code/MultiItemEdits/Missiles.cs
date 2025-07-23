using System;
using System.Collections.Generic;
using System.Text;
using LordsItemEdits.ItemEdits;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace LordsItemEdits.MultiItemEdits
{
    internal class Missiles
    {
        [MonoDetourTargets(typeof(MissileUtils))]
        internal static class MonoDetourEdits
        {
            [MonoDetourHookInitialize]
            internal static void Setup()
            {
                // this affects both engi harpoon and DML this is in here
                if (ConfigOptions.PocketICBM.ChangeDMLEffect.Value || ConfigOptions.PocketICBM.ChangeEngiHarpoonEffect.Value)
                {
                    MonoDetourHooks.RoR2.MissileUtils.FireMissile_UnityEngine_Vector3_RoR2_CharacterBody_RoR2_ProcChainMask_UnityEngine_GameObject_System_Single_System_Boolean_UnityEngine_GameObject_RoR2_DamageColorIndex_UnityEngine_Vector3_System_Single_System_Boolean.ILHook(ChangeICBMEffect);
                }
            }

            private static void ChangeICBMEffect(ILManipulationInfo info)
            {
                ILWeaver w = new(info);


                // near the end of "float num3 = Mathf.Max(1f, 1f + 0.5f * (float)(valueOrDefault - 1));"
                w.MatchRelaxed(
                    x => x.MatchMul(),
                    x => x.MatchAdd(),
                    x => x.MatchCall(out _) && w.SetCurrentTo(x),
                    x => x.MatchStloc(1)
                ).ThrowIfFailure()
                .InsertAfterCurrent(
                    w.Create(OpCodes.Ldarg_1),
                    w.CreateCall(ReplaceOldICBMDamageMult)
                );


                // after "ProjectileManager.instance.FireProjectile(fireProjectileInfo);"
                w.MatchRelaxed(
                    x => x.MatchCall(out _),
                    x => x.MatchLdloc(4),
                    x => x.MatchCallvirt(out _) && w.SetCurrentTo(x)
                ).ThrowIfFailure()
                .InsertAfterCurrent(
                    // its that easy
                    w.Create(OpCodes.Ret)
                );
            }

            private static float ReplaceOldICBMDamageMult(float oldDamageMult, CharacterBody attackerBody)
            {
                return PocketICBM.GetICBMDamageMult(attackerBody);
            }
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
            missileOrb.damageValue *= PocketICBM.GetICBMDamageMult(attackerBody);
            OrbManager.instance.AddOrb(missileOrb);
            // the orb doesn't play a sound on fire and editing the assets isn't working so
            Util.PlaySound("Play_item_proc_missile_fire", attackerBody.gameObject);
        }
    }
}