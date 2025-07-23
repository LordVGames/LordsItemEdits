using System;   
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using RoR2;

namespace LordsItemEdits.ItemEdits
{
    [MonoDetourTargets(typeof(GlobalEventManager))]
    internal class Plimp
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            MonoDetourHooks.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(ReplacePlimpICBMEffect);
        }



        private static void ReplacePlimpICBMEffect(ILManipulationInfo info)
        {
            ILWeaver w = new(info);


            // near the end of "float num10 = Mathf.Max(1f, 1f + 0.5f * (float)(valueOrDefault - 1));"
            w.MatchRelaxed(
                x => x.MatchMul(),
                x => x.MatchAdd(),
                x => x.MatchCall(out _) && w.SetCurrentTo(x),
                x => x.MatchStloc(47)
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_0),
                w.Create(OpCodes.Ldloc, 46),
                w.CreateCall(ChangeICBMDamageMultIfNeeded)
            );


            // near the end of "int num13 = ((valueOrDefault > 0) ? 3 : 1);" finishes
            w.MatchRelaxed(
                x => x.MatchLdcI4(1),
                x => x.MatchBr(out _),
                x => x.MatchLdcI4(3) && w.SetCurrentTo(x),
                x => x.MatchStloc(50) 
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.CreateCall(ChangePlimpAmountIfNeeded)
            );
        }

        private static float ChangeICBMDamageMultIfNeeded(float oldDamageMult, CharacterBody characterBody, int allowICBMEffectsAsInt)
        {
            if (ConfigOptions.PocketICBM.ChangePlimpEffect.Value && allowICBMEffectsAsInt > 0)
            {
                return PocketICBM.GetICBMDamageMult(characterBody);
            }
            return oldDamageMult;
        }

        private static int ChangePlimpAmountIfNeeded(int currentPlimpAmount)
        {
            if (ConfigOptions.PocketICBM.ChangePlimpEffect.Value && currentPlimpAmount == 3)
            {
                return 1;
            }
            return currentPlimpAmount;
        }
    }
}