using System;   
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using RoR2;
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(GlobalEventManager))]
internal static class Plimp
{
    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.PocketICBM.ChangePlasmaShrimpEffect.Value)
        {
            return;
        }

        Mdh.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(ReplacePlimpICBMEffect);
    }



    private static void ReplacePlimpICBMEffect(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        int stlocValue = 0;


        // near the end of "float num6 = Mathf.Max(1f, 1f + 0.5f * (float)(num5 - 1));"
        w.MatchRelaxed(
            x => x.MatchMul(),
            x => x.MatchAdd(),
            x => x.MatchCall(out _) && w.SetCurrentTo(x),
            x => x.MatchStloc(out stlocValue),
            x => x.MatchLdcR4(0.4f),
            x => x.MatchLdloc(out _)
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.Create(OpCodes.Ldloc_0), // CharacterBody
            w.Create(OpCodes.Ldloc, stlocValue - 1), // allowICBMEffectsAsInt
            w.CreateCall(ChangeICBMDamageMultIfNeeded)
        );


        // near the end of "int num7 = ((num5 <= 0) ? 1 : 3);"
        w.MatchRelaxed(
            x => x.MatchLdcI4(1),
            x => x.MatchBr(out _),
            x => x.MatchLdcI4(3) && w.SetCurrentTo(x),
            x => x.MatchStloc(out _),
            x => x.MatchLdcI4(0),
            x => x.MatchStloc(out _),
            x => x.MatchBr(out _)
        ).ThrowIfFailure()
        .InsertAfterCurrent(
            w.CreateCall(ChangePlimpAmountIfNeeded)
        );
    }

    private static float ChangeICBMDamageMultIfNeeded(float oldDamageMult, CharacterBody characterBody, int allowICBMEffectsAsInt)
    {
        if (ConfigOptions.PocketICBM.ChangePlasmaShrimpEffect.Value && allowICBMEffectsAsInt > 0)
        {
            return PocketICBM.GetICBMDamageMult(characterBody);
        }
        return oldDamageMult;
    }

    private static int ChangePlimpAmountIfNeeded(int currentPlimpAmount)
    {
        if (ConfigOptions.PocketICBM.ChangePlasmaShrimpEffect.Value && currentPlimpAmount == 3)
        {
            return 1;
        }
        return currentPlimpAmount;
    }
}