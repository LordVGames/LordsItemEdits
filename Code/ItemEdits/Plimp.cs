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
                w.CreateCall(RemoveOldICBMDamageMultIfNeeded)
            );


            // near the end of "float num12 = Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, num11) * num10;"
            w.MatchRelaxed(
                x => x.MatchLdloc(47),
                x => x.MatchMul() && w.SetCurrentTo(x),
                x => x.MatchStloc(49)
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.Create(OpCodes.Ldloc_0),
                w.Create(OpCodes.Ldloc, 46),
                w.CreateCall(ChangePlimpDamageIfNeeded)
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

        private static float RemoveOldICBMDamageMultIfNeeded(float oldDamageMult)
        {
            // removing it in the sense of making it not do anything
            if (ConfigOptions.PocketICBM.ChangePlimpEffect.Value)
            {
                return 1f;
            }
            return oldDamageMult;
        }

        private static float ChangePlimpDamageIfNeeded(float plimpDamage, CharacterBody characterBody, int allowPlimpAsInt)
        {
            if (ConfigOptions.PocketICBM.ChangePlimpEffect.Value && allowPlimpAsInt > 0)
            {
                return plimpDamage * PocketICBM.GetICBMDamageMult(characterBody);
            }
            return plimpDamage;
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