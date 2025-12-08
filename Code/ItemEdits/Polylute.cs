using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(GlobalEventManager))]
internal class Polylute
{
    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.Polylute.EnableEdit.Value)
        {
            return;
        }

        ModLanguage.LangFilesToLoad.Add("Polylute");
        Mdh.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(SwapStackingHitsForDamage);
    }

    private static void SwapStackingHitsForDamage(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        int damageValueVariableNumber = 79;

        // going to end of:
        // float damageValue4 = Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, damageCoefficient5);
        w.MatchRelaxed(
            x => x.MatchLdloc(78),
            x => x.MatchCall("RoR2.Util", "OnHitProcDamage"),
            x => x.MatchStloc(damageValueVariableNumber) && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.Create(OpCodes.Ldloc, 76), // load item count (found near line matched to)
            w.Create(OpCodes.Ldloc, damageValueVariableNumber), // load damage
            w.CreateCall(DoDamageMult),
            w.Create(OpCodes.Stloc, damageValueVariableNumber) // set damage again
        );


        int voidLightningOrbVariableNumber = 80;
        // at the end of line:
        // voidLightningOrb.totalStrikes = 3 * itemCountEffective9;
        w.MatchRelaxed(
            x => x.MatchLdloc(voidLightningOrbVariableNumber),
            x => x.MatchLdcI4(3),
            x => x.MatchLdloc(76),
            x => x.MatchMul(),
            x => x.MatchStfld(out _) && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.Create(OpCodes.Ldloc, voidLightningOrbVariableNumber), // load VoidLightningOrb
            w.CreateCall(ResetVoidLightningOrbStrikeCount)
        );
        
        //w.LogILInstructions();
    }

    private static float DoDamageMult(int polyluteCount, float damage)
    {
        return damage * polyluteCount;
    }

    private static void ResetVoidLightningOrbStrikeCount(VoidLightningOrb voidLightningOrb)
    {
        voidLightningOrb.totalStrikes = 3;
    }
}