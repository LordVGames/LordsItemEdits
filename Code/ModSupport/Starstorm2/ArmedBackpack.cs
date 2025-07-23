using System;
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
    // it's publicized, shut up
    [MonoDetourTargets(typeof(SS2.Items.ArmedBackpack.Behavior))]
    internal class ArmedBackpack
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            On.SS2.Items.ArmedBackpack.Behavior.OnTakeDamageServer.ILHook(ReplaceMissileWithOrb);
        }

        private static void ReplaceMissileWithOrb(ILManipulationInfo info)
        {
            ILWeaver w = new(info);
            ILLabel skipFireMissile = w.DefineLabel();


            w.MatchRelaxed(
                x => x.MatchLdloc(2),
                x => x.MatchMul(),
                x => x.MatchStloc(3) && w.SetCurrentTo(x)
            ).ThrowIfFailure();
            w.InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_3),
                w.Create(OpCodes.Ldarg_1),
                w.CreateCall(FireMissileOrbIfApplicable),
                w.Create(OpCodes.Brtrue, skipFireMissile)
            );


            w.MatchRelaxed(
                x => x.MatchCall("RoR2.MissileUtils", "FireMissile") && w.SetCurrentTo(x),
                x => x.MatchRet()
            ).ThrowIfFailure();
            w.MarkLabelToCurrentNext(skipFireMissile);


            //w.LogILInstructions();
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