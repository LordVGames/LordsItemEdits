﻿using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using RoR2;
using LordsItemEdits.MultiItemEdits;

namespace LordsItemEdits.ModSupport.Starstorm2
{
    [MonoDetourTargets(typeof(SS2.Items.ArmedBackpack.Behavior))]
    internal static class ArmedBackpack
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.SS2Items.ArmedBackpack.EnableEdit.Value)
            {
                return;
            }

            MonoDetourHooks.SS2.Items.ArmedBackpack.Behavior.OnTakeDamageServer.ILHook(ReplaceMissileWithOrb);
        }

        private static void ReplaceMissileWithOrb(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipFireMissile = w.DefineLabel();


            //w.LogILInstructions();
            if (ConfigOptions.SS2Items.HookForBetaVersion.Value)
            {
                // going to after line:
                // float num4 = victimBody.damage * num3;
                w.MatchRelaxed(
                    x => x.MatchLdloc(5),
                    x => x.MatchMul(),
                    x => x.MatchStloc(6) && w.SetCurrentTo(x)
                ).ThrowIfFailure()
                .InsertAfterCurrent(
                    w.Create(OpCodes.Ldloc, 6),
                    w.Create(OpCodes.Ldarg_1),
                    w.CreateCall(FireMissileOrbIfApplicable),
                    w.Create(OpCodes.Brtrue, skipFireMissile)
                );


                // going to before line:
                // MissileUtils.FireMissile
                w.MatchRelaxed(
                    x => x.MatchCall("RoR2.MissileUtils", "FireMissile") && w.SetCurrentTo(x),
                    // why does nop even exist???
                    x => x.MatchNop(),
                    x => x.MatchNop(),
                    x => x.MatchNop(),
                    x => x.MatchRet()
                ).ThrowIfFailure()
                .MarkLabelToCurrentNext(skipFireMissile);
            }
            else
            {
                // going to after line:
                // float num3 = victimBody.damage * num2;
                w.MatchRelaxed(
                    x => x.MatchLdloc(2),
                    x => x.MatchMul(),
                    x => x.MatchStloc(3) && w.SetCurrentTo(x)
                ).ThrowIfFailure()
                .InsertAfterCurrent(
                    w.Create(OpCodes.Ldloc_3),
                    w.Create(OpCodes.Ldarg_1),
                    w.CreateCall(FireMissileOrbIfApplicable),
                    w.Create(OpCodes.Brtrue, skipFireMissile)
                );


                // going to before line:
                // MissileUtils.FireMissile
                w.MatchRelaxed(
                    x => x.MatchCall("RoR2.MissileUtils", "FireMissile") && w.SetCurrentTo(x),
                    x => x.MatchRet()
                ).ThrowIfFailure()
                .MarkLabelToCurrentNext(skipFireMissile);
            }
        }

        private static bool FireMissileOrbIfApplicable(float missileDamage, DamageReport damageReport)
        {
            // this prevents it from firing into blood shrines and i guess yourself/teammates if that lunar active is involved
            if (damageReport.victimTeamIndex != TeamIndex.Player)
            {
                return false;
            }

            Missiles.FireMissileOrb(damageReport.victimBody, missileDamage, damageReport.damageInfo, damageReport.attackerBody, false);
            return true;
        }
    }
}