using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using static UnityEngine.Object;
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(ExtraLifeVoidManager), GenerateControlFlowVariants = true)]
[MonoDetourTargets(typeof(CharacterMaster))]
[MonoDetourTargets(typeof(CharacterBody), GenerateControlFlowVariants = true)]
internal static class VoidDios
{
    #region Needed Assets
    private static readonly AssetReferenceT<ItemDef> _voidDiosItemAssetReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_ExtraLifeVoid.ExtraLifeVoid_asset);
    private static readonly AssetReferenceT<ItemDef> _cutHpItemDefAssetReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_CutHp.CutHp_asset);
    private static ItemDef _cutHpItemDef;

    private static readonly AssetReferenceT<GameObject> _reaverAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.NullifierAllyMaster_prefab);
    private static GameObject _reaverAllyMasterPrefab;
    private static readonly AssetReferenceT<GameObject> _reaverAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Nullifier.NullifierAllyBody_prefab); 

    private static readonly AssetReferenceT<GameObject> _jailerAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidJailer.VoidJailerAllyMaster_prefab);
    private static GameObject _jailerAllyMasterPrefab;
    private static readonly AssetReferenceT<GameObject> _jailerAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab);

    private static readonly AssetReferenceT<GameObject> _devastatorAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyMaster_prefab);
    private static GameObject _devastatorAllyMasterPrefab;
    private static readonly AssetReferenceT<GameObject> _devastatorAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab);
    #endregion


    private static readonly FixedConditionalWeakTable<CharacterMaster, LieVoidDiosInfo> _lieVoidDiosInfoTable = [];
    private class LieVoidDiosInfo
    {
        internal GameObject OriginalBodyPrefab;
        internal string NameOfSceneFirstRevivedIn;
    }


    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.VoidDios.EnableEdit.Value)
        {
            return;
        }

        AssignMasterPrefabs();
        EditVoidBodyPrefabs();
        EditItemPrefabs();
        Mdh.RoR2.Items.ExtraLifeVoidManager.GetNextBodyPrefab.ControlFlowPrefix(ExtraLifeVoidManager_GetNextBodyPrefab);
        Mdh.RoR2.CharacterMaster.RespawnExtraLifeVoid.ILHook(CharacterMaster_RespawnExtraLifeVoid);
        Mdh.RoR2.CharacterBody.Start.ControlFlowPrefix(CharacterBody_Start);
        ModLanguage.LangFilesToLoad.Add("VoidDios");
    }


    private static void AssignMasterPrefabs()
    {
        AssetAsyncReferenceManager<GameObject>.LoadAsset(_reaverAllyMasterPrefabReference).Completed += (handle) =>
        {
            _reaverAllyMasterPrefab = handle.Result;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_reaverAllyMasterPrefabReference);
        };

        AssetAsyncReferenceManager<GameObject>.LoadAsset(_jailerAllyMasterPrefabReference).Completed += (handle) =>
        {
            _jailerAllyMasterPrefab = handle.Result;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_jailerAllyMasterPrefabReference);
        };

        AssetAsyncReferenceManager<GameObject>.LoadAsset(_devastatorAllyMasterPrefabReference).Completed += (handle) =>
        {
            _devastatorAllyMasterPrefab = handle.Result;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_devastatorAllyMasterPrefabReference);
        };
    }


    private static void EditVoidBodyPrefabs()
    {
        // void allies are playable now so i need to increase all of their interaction ranges since they can't reach anything normally
        AssetAsyncReferenceManager<GameObject>.LoadAsset(_reaverAllyBodyPrefabReference).Completed += (handle) =>
        {
            Interactor reaverAllyInteractor = handle.Result.GetComponent<Interactor>();
            reaverAllyInteractor.maxInteractionDistance = 9;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_reaverAllyBodyPrefabReference);
        };

        AssetAsyncReferenceManager<GameObject>.LoadAsset(_jailerAllyBodyPrefabReference).Completed += (handle) =>
        {
            Interactor jailerAllyInteractor = handle.Result.GetComponent<Interactor>();
            jailerAllyInteractor.maxInteractionDistance = 12;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_jailerAllyBodyPrefabReference);
        };

        AssetAsyncReferenceManager<GameObject>.LoadAsset(_devastatorAllyBodyPrefabReference).Completed += (handle) =>
        {
            Interactor devastatorAllyInteractor = handle.Result.GetComponent<Interactor>();
            devastatorAllyInteractor.maxInteractionDistance = 15;
            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_devastatorAllyBodyPrefabReference);
        };
    }


    private static void EditItemPrefabs()
    {
        // no way i'm letting engi turrets get this new void dios lmao
        AssetAsyncReferenceManager<ItemDef>.LoadAsset(_voidDiosItemAssetReference).Completed += (handle) =>
        {
            ItemDef voidDiosItemDef = handle.Result;
            voidDiosItemDef.tags = [.. voidDiosItemDef.tags, ItemTag.CannotCopy];
            AssetAsyncReferenceManager<ItemDef>.UnloadAsset(_voidDiosItemAssetReference);
        };

        // why tf is this not already set as hidden
        AssetAsyncReferenceManager<ItemDef>.LoadAsset(_cutHpItemDefAssetReference).Completed += (handle) =>
        {
            _cutHpItemDef = handle.Result;
            _cutHpItemDef.hidden = true;
            AssetAsyncReferenceManager<ItemDef>.UnloadAsset(_voidDiosItemAssetReference);
        };
    }




    private static ReturnFlow ExtraLifeVoidManager_GetNextBodyPrefab(ref GameObject returnValue)
    {
        string[] voidBodyList = ["NullifierAllyBody", "VoidJailerAllyBody", "VoidMegaCrabAllyBody"];
        GameObject chosenBody = null;
        if (ConfigOptions.VoidDios.AllowRespawnAsVoidReaver.Value)
        {
            chosenBody = BodyCatalog.FindBodyPrefab(voidBodyList[ExtraLifeVoidManager.rng.RangeInt(0, voidBodyList.Length)]);
        }
        else
        {
            chosenBody = BodyCatalog.FindBodyPrefab(voidBodyList[ExtraLifeVoidManager.rng.RangeInt(1, voidBodyList.Length)]);
        }
        returnValue = chosenBody;
        return ReturnFlow.SkipOriginal;
    }


    private static void CharacterMaster_RespawnExtraLifeVoid(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        // going into start of line:
        // Respawn(vector, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), wasRevivedMidStage: true);
        w.MatchRelaxed(
            x => x.MatchLdarg(0) && w.SetCurrentTo(x),
            x => x.MatchLdloc(0),
            x => x.MatchLdcR4(0),
            x => x.MatchLdcR4(0)
        ).ThrowIfFailure()
        .InsertBeforeCurrentStealLabels(
            w.Create(OpCodes.Ldarg_0),
            w.CreateDelegateCall((CharacterMaster characterMaster) =>
            {
                if (!_lieVoidDiosInfoTable.TryGetValue(characterMaster, out _))
                {
                    _lieVoidDiosInfoTable.Add(characterMaster, new LieVoidDiosInfo { OriginalBodyPrefab = characterMaster.bodyPrefab, NameOfSceneFirstRevivedIn = SceneManager.GetActiveScene().name });
                    Log.Info($"Giving {characterMaster.GetBody()?.name} a CutHp due to them using up a void dios for the first time this stage");
                    characterMaster.inventory?.GiveItemPermanent(_cutHpItemDef.itemIndex);
                }

                GameObject voidAllyBodyPrefab = ExtraLifeVoidManager.GetNextBodyPrefab(); // the method to pick a void guy to respawn as is still there, just unused
                characterMaster.bodyPrefab = voidAllyBodyPrefab;

                BaseAI originalBaseAI = characterMaster.GetComponent<BaseAI>();
                GameObject voidAllyMasterPrefab = GetVoidAllyMasterPrefabFromBodyPrefab(voidAllyBodyPrefab);
                if (originalBaseAI != null && voidAllyMasterPrefab != null)
                {
                    ReplaceAISkillDrivers(characterMaster, originalBaseAI, voidAllyMasterPrefab); // fixes the ai being lobotomized since the ai doesn't change with their body
                }

                //return characterMaster;
            })
        );


        ILLabel skipItemVoiding = w.DefineLabel();

        // go before foreach line
        w.MatchRelaxed(
            x => x.MatchLdloca(4) && w.SetCurrentTo(x),
            x => x.MatchCall(out _),
            x => x.MatchStloc(5)
        ).ThrowIfFailure()
        .InsertBeforeCurrentStealLabels(
          w.Create(OpCodes.Br, skipItemVoiding)  
        );

        // go to the end of the foreach block
        w.MatchRelaxed(
            x => x.MatchCallvirt<IDisposable>("Dispose"),
            x => x.MatchEndfinally() && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .MarkLabelToCurrentNext(skipItemVoiding);
    }


    private static ReturnFlow CharacterBody_Start(CharacterBody self)
    {
        if (self == null || self.master == null)
        {
            return ReturnFlow.None;
        }
        if (!_lieVoidDiosInfoTable.TryGetValue(self.master, out var lieVoidDiosInfo))
        {
            return ReturnFlow.None;
        }
        if (lieVoidDiosInfo.NameOfSceneFirstRevivedIn == SceneManager.GetActiveScene().name)
        {
            return ReturnFlow.None;
        }


        self.master.bodyPrefab = lieVoidDiosInfo.OriginalBodyPrefab;
        _lieVoidDiosInfoTable.Remove(self.master);
        if (self.master.inventory != null)
        {
            if (self.master.inventory.GetItemCountEffective(_cutHpItemDef.itemIndex) > 0)
            {
                Log.Info($"Removing a CutHp from {self.name} due to starting in a new stage after being revived by a void dios in the previous stage");
                self.master.inventory.RemoveItemPermanent(_cutHpItemDef.itemIndex);
            }
            else
            {
                Log.Warning("CutHP count was less than 1 when there should've been 1! Something might've gone wrong?");
            }
        }
        else
        {
            Log.Error("SELF MASTER INVENTORY IN CharacterBody_Start WAS NULL!!!!! NOT GOOOD!!!!! YOU'RE STUCK WITH A CUTHP ITEM NOW!!!");
        }


        self.master.Respawn(self.master.bodyPrefab.name);
        return ReturnFlow.SkipOriginal;
    }


    private static GameObject GetVoidAllyMasterPrefabFromBodyPrefab(GameObject voidAllyBodyPrefab)
    {
        GameObject masterPrefab;
        switch (voidAllyBodyPrefab.name)
        {
            case "NullifierAllyBody":
                masterPrefab = _reaverAllyMasterPrefab;
                break;
            case "VoidJailerAllyBody":
                masterPrefab = _jailerAllyMasterPrefab;
                break;
            case "VoidMegaCrabAllyBody":
                masterPrefab = _devastatorAllyMasterPrefab;
                break;
            default:
                Log.Error("Could not get master prefab for void ally body! This really shouldn't happen ever!");
                return null;
        }
        return masterPrefab;
    }


    // from DestroyedClone's TransformingAIFix/TransformingFix
    private static void ReplaceAISkillDrivers(CharacterMaster characterMaster, BaseAI baseAI, GameObject newCharacterMasterPrefab)
    {
        //Chat.AddMessage($"{characterMaster.name} has transformed into {newCharacterMasterPrefab.name}");
        foreach (var skillDriver in characterMaster.GetComponents<AISkillDriver>())
        {
            Destroy(skillDriver);
        }
        AISkillDriver[] listOfPrefabDrivers = newCharacterMasterPrefab.GetComponents<AISkillDriver>();
        List<AISkillDriver> newSkillDrivers = [];

        foreach (var skillDriver in listOfPrefabDrivers)
        {
            var newDriver = characterMaster.gameObject.AddComponent<AISkillDriver>();
            newDriver.activationRequiresAimConfirmation = skillDriver.activationRequiresAimConfirmation;
            newDriver.activationRequiresAimTargetLoS = skillDriver.activationRequiresAimTargetLoS;
            newDriver.activationRequiresTargetLoS = skillDriver.activationRequiresTargetLoS;
            newDriver.aimType = skillDriver.aimType;
            newDriver.buttonPressType = skillDriver.buttonPressType;
            newDriver.customName = skillDriver.customName;
            newDriver.driverUpdateTimerOverride = skillDriver.driverUpdateTimerOverride;
            newDriver.ignoreNodeGraph = skillDriver.ignoreNodeGraph;
            newDriver.maxDistance = skillDriver.maxDistance;
            newDriver.maxTargetHealthFraction = skillDriver.maxTargetHealthFraction;
            newDriver.maxUserHealthFraction = skillDriver.maxUserHealthFraction;
            newDriver.minDistance = skillDriver.minDistance;
            newDriver.minTargetHealthFraction = skillDriver.minTargetHealthFraction;
            newDriver.minUserHealthFraction = skillDriver.minUserHealthFraction;
            newDriver.moveInputScale = skillDriver.moveInputScale;
            newDriver.movementType = skillDriver.movementType;
            newDriver.moveTargetType = skillDriver.moveTargetType;
            newDriver.nextHighPriorityOverride = skillDriver.nextHighPriorityOverride;
            newDriver.noRepeat = skillDriver.noRepeat;
            newDriver.requiredSkill = skillDriver.requiredSkill;
            newDriver.requireEquipmentReady = skillDriver.requireEquipmentReady;
            newDriver.requireSkillReady = skillDriver.requireSkillReady;
            newDriver.resetCurrentEnemyOnNextDriverSelection = skillDriver.resetCurrentEnemyOnNextDriverSelection;
            newDriver.selectionRequiresAimTarget = skillDriver.selectionRequiresAimTarget;
            newDriver.selectionRequiresOnGround = skillDriver.selectionRequiresOnGround;
            newDriver.selectionRequiresTargetLoS = skillDriver.selectionRequiresTargetLoS;
            newDriver.shouldFireEquipment = skillDriver.shouldFireEquipment;
            newDriver.shouldSprint = skillDriver.shouldSprint;
            newDriver.buttonPressType = skillDriver.buttonPressType;
            newDriver.skillSlot = skillDriver.skillSlot;
            newDriver.name = $"{skillDriver.name}(Clone)"; // keeping things the same as if it was spawned in
            newSkillDrivers.Add(newDriver);
        }
        AISkillDriver[] array = [.. newSkillDrivers];
        baseAI.skillDrivers = array;

        EntityStateMachine esm = characterMaster.GetComponent<EntityStateMachine>();
        EntityStateMachine customESM = newCharacterMasterPrefab.GetComponent<EntityStateMachine>();
        esm.customName = customESM.customName;
        esm.initialStateType = customESM.initialStateType;
        esm.mainStateType = customESM.mainStateType;
        esm.nextState = customESM.nextState;
    }
}