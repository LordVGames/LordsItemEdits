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

namespace LordsItemEdits.ItemEdits
{
    internal class Polylute
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.Polylute.EnableEdit.Value)
            {
                return;
            }

            LanguageAPI.AddOverlayPath(ModUtil.GetLangFileLocation("Polylute"));
            MonoDetourHooks.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(SwapStackingHitsForDamage);
        }

        private static void SwapStackingHitsForDamage(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            // going after:
            // float num21 = Util.OnHitProcDamage(damageInfo.damage, characterBody.damage, num20);
            w.MatchRelaxed(
                x => x.MatchLdloc(67),
                x => x.MatchCall("RoR2.Util", "OnHitProcDamage"),
                x => x.MatchStloc(68) && w.SetCurrentTo(x)
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.Create(OpCodes.Ldloc, 65), // load item count
                w.Create(OpCodes.Ldloc, 68), // load damage
                w.CreateCall(DoDamageMult),
                w.Create(OpCodes.Stloc, 68)
            );


            // in the middle of:
            // voidLightningOrb.totalStrikes = 3 * itemCount6;
            w.MatchRelaxed(
                x => x.MatchLdloc(69),
                x => x.MatchLdcI4(3),
                x => x.MatchLdloc(65),
                x => x.MatchMul(),
                x => x.MatchStfld(out _) && w.SetCurrentTo(x)
            ).ThrowIfFailure()
            .InsertAfterCurrent(
                w.Create(OpCodes.Ldloc, 69), // load VoidLightningOrb
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
}
