﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoDetour;
using MonoDetour.HookGen;
using MonoDetour.Cil;
using R2API;
using RoR2;
using RoR2.ContentManagement;

namespace LordsItemEdits.ItemEdits
{
    internal static class ExecutiveCard
    {
        internal static void Setup()
        {
            if (!ConfigOptions.ExecutiveCard.EnableEdit.Value)
            {
                return;
            }

            LanguageAPI.AddOverlayPath(ModUtil.GetLangFileLocation("ExecutiveCard"));
            CreditScoreBuff.SetupBuff();
            EquipmentDefEdits.Setup();
            ILHooks.Setup();
            OnHooks.Setup();
        }



        private static class CreditScoreBuff
        {

            private static readonly AssetReferenceT<Sprite> _creditCardIconSpriteReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_MultiShopCard.texExecutiveCardIcon_png);
            internal static BuffDef bdCreditScore;

            internal static void SetupBuff()
            {
                bdCreditScore = ScriptableObject.CreateInstance<BuffDef>();
                bdCreditScore.name = "bdCreditScore";
                bdCreditScore.buffColor = Color.white;
                bdCreditScore.canStack = true;
                bdCreditScore.isDebuff = false;
                bdCreditScore.flags = BuffDef.Flags.ExcludeFromNoxiousThorns;
                bdCreditScore.ignoreGrowthNectar = true;
                // idek if this works lmao
                bdCreditScore.startSfx = CreateNetworkSoundEventDef("Play_item_proc_moneyOnKill_loot");
                AssetAsyncReferenceManager<Sprite>.LoadAsset(_creditCardIconSpriteReference).Completed += (handle) =>
                {
                    bdCreditScore.iconSprite = handle.Result;
                    AssetAsyncReferenceManager<Sprite>.UnloadAsset(_creditCardIconSpriteReference);
                };
                ContentAddition.AddBuffDef(bdCreditScore);
            }

            // ty nuxlar
            private static NetworkSoundEventDef CreateNetworkSoundEventDef(string eventName)
            {
                NetworkSoundEventDef networkSoundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
                networkSoundEventDef.akId = AkSoundEngine.GetIDFromString(eventName);
                networkSoundEventDef.eventName = eventName;

                ContentAddition.AddNetworkSoundEventDef(networkSoundEventDef);

                return networkSoundEventDef;
            }
        }



        private static class EquipmentDefEdits
        {
            private static readonly AssetReferenceT<EquipmentDef> _creditCardEquipmentDefReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_MultiShopCard.MultiShopCard_asset);
            internal static void Setup()
            {
                MakeCreditCardAnActualEquipment();
            }

            private static void MakeCreditCardAnActualEquipment()
            {
                AssetAsyncReferenceManager<EquipmentDef>.LoadAsset(_creditCardEquipmentDefReference).Completed += (handle) =>
                {
                    if (ConfigOptions.ExecutiveCard.AddCreditCardToBottledChaos.Value)
                    {
                        handle.Result.canBeRandomlyTriggered = true;
                    }
                    handle.Result.enigmaCompatible = true;
                    handle.Result.cooldown = 80;

                    AssetAsyncReferenceManager<EquipmentDef>.UnloadAsset(_creditCardEquipmentDefReference);
                };
            }
        }



        private static class ILHooks
        {
            internal static void Setup()
            {
                MultiShopCardUtils.Setup();
            }

            [MonoDetourTargets(typeof(RoR2.Items.MultiShopCardUtils))]
            private static class MultiShopCardUtils
            {
                [MonoDetourHookInitialize]
                internal static void Setup()
                {
                    MonoDetourHooks.RoR2.Items.MultiShopCardUtils.OnPurchase.ILHook(UseCreditScoreBuffPls);
                }

                private static void UseCreditScoreBuffPls(ILManipulationInfo info)
                {
                    ILWeaver w = new(info);
                    ILLabel exitCode = w.DefineLabel();
                    ILLabel skipFirstBadChecks = w.DefineLabel();
                    ILLabel skipSecondBadChecks = w.DefineLabel();

                    // going a little into a long line that starts with:
                    // if (activatorMaster
                    w.MatchRelaxed(
                        x => x.MatchLdloc(0),
                        x => x.MatchCallvirt<CharacterMaster>("get_hasBody"),
                        x => x.MatchBrfalse(out exitCode) && w.SetCurrentTo(x)
                    ).ThrowIfFailure()
                    .InsertAfterCurrent(
                        w.Create(OpCodes.Br, skipFirstBadChecks)
                    );


                    // going to end of same line above
                    w.MatchRelaxed(
                        x => x.MatchLdsfld("RoR2.DLC1Content/Equipment", "MultiShopCard"),
                        x => x.MatchCallvirt<EquipmentDef>("get_equipmentIndex"),
                        x => x.MatchBneUn(out _) && w.SetCurrentTo(x)
                    ).ThrowIfFailure()
                    .MarkLabelToCurrentNext(skipFirstBadChecks);


                    // going to before:
                    // if (body.equipmentSlot.stock > 0)
                    w.MatchRelaxed(
                        x => x.MatchLdloc(0),
                        x => x.MatchCallvirt<CharacterMaster>("GetBody"),
                        x => x.MatchStloc(1) && w.SetCurrentTo(x)
                    ).ThrowIfFailure()
                    .InsertAfterCurrent(
                        w.Create(OpCodes.Ldloc_1),
                        w.CreateCall(DoesBodyHaveCreditScore),
                        w.Create(OpCodes.Brfalse, exitCode),
                        w.Create(OpCodes.Br, skipSecondBadChecks)
                    );


                    // going to after the line above
                    w.MatchRelaxed(
                        x => x.MatchLdloc(1),
                        x => x.MatchCallvirt<CharacterBody>("get_equipmentSlot"),
                        x => x.MatchCallvirt<EquipmentSlot>("get_stock"),
                        x => x.MatchLdcI4(0),
                        x => x.MatchBle(out _) && w.SetCurrentTo(x)
                    ).ThrowIfFailure()
                    .MarkLabelToCurrentNext(skipSecondBadChecks)
                    .CurrentToNext()
                    .InsertAfterCurrent(
                        w.Create(OpCodes.Ldloc_1),
                        w.CreateCall(RemoveCreditScoreBuffFromBody)
                    );

                    //w.LogILInstructions();
                }

                private static bool DoesBodyHaveCreditScore(CharacterBody characterBody)
                {
                    return characterBody.HasBuff(CreditScoreBuff.bdCreditScore);
                }

                private static void RemoveCreditScoreBuffFromBody(CharacterBody characterBody)
                {
                    if (characterBody.HasBuff(CreditScoreBuff.bdCreditScore))
                    {
                        characterBody.RemoveBuff(CreditScoreBuff.bdCreditScore);
                    }
                }
            }
        }



        private static class OnHooks
        {
            internal static void Setup()
            {
                On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            }

            private static bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
            {
                if (equipmentDef != DLC1Content.Equipment.MultiShopCard)
                {
                    return orig(self, equipmentDef);
                }

                AddCreditScoreStacks(self.characterBody);
                return true;
            }

            private static void AddCreditScoreStacks(CharacterBody characterBody)
            {
                // i really have to AddBuff on 2 separate lines................ts pmo......................................................
                characterBody.AddBuff(CreditScoreBuff.bdCreditScore);
                characterBody.AddBuff(CreditScoreBuff.bdCreditScore);
                Util.PlaySound("Play_item_proc_moneyOnKill_loot", characterBody.gameObject);
            }
        }
    }
}