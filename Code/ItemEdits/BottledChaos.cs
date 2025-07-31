using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits.ItemEdits
{
    [MonoDetourTargets(typeof(Inventory))]
    internal static class BottledChaos
    {
        [MonoDetourHookInitialize]
        internal static void Setup()
        {
            if (!ConfigOptions.BottledChaos.EnableEdit.Value)
            {
                return;
            }

            LanguageAPI.AddOverlayPath(ModUtil.GetLangFileLocation("BottledChaos"));
            MonoDetourHooks.RoR2.Inventory.CalculateEquipmentCooldownScale.ILHook(SetupBhaosCooldownReduction);
        }

        private static void SetupBhaosCooldownReduction(ILManipulationInfo info)
        {
            ILWeaver w = new(info);

            // before "return num;"
            w.MatchRelaxed(
                x => x.MatchLdloc(3) && w.SetCurrentTo(x),
                x => x.MatchRet()
            ).ThrowIfFailure()
            .InsertBeforeCurrentStealLabels(
                w.Create(OpCodes.Ldarg_0),
                w.Create(OpCodes.Ldloc_3),
                w.CreateCall(AddBhaosCooldownReduction),
                w.Create(OpCodes.Stloc_3)
            );
        }

        private static float AddBhaosCooldownReduction(Inventory inventory, float currentCooldownReduction)
        {
            if (inventory.GetItemCount(DLC1Content.Items.RandomEquipmentTrigger) > 0)
            {
                return currentCooldownReduction *= 0.65f;
            }
            return currentCooldownReduction;
        }
    }
}