using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
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

namespace LordsItemEdits.ItemEdits
{
    internal static class VoidDios
    {
        private static AssetReferenceT<ItemDef> _voidDiosItemAssetReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_ExtraLifeVoid.ExtraLifeVoid_asset);
        private static AssetReferenceT<ItemDef> _cutHpItemDefAssetReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_Base_CutHp.CutHp_asset);
        private static ItemDef _cutHpItemDef;

        private static AssetReferenceT<GameObject> _reaverAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Nullifier.NullifierAllyMaster_prefab);
        private static GameObject _reaverAllyMasterPrefab;
        private static AssetReferenceT<GameObject> _reaverAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Nullifier.NullifierAllyBody_prefab); 
        //private static GameObject _reaverAllyBodyPrefab;

        private static AssetReferenceT<GameObject> _jailerAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidJailer.VoidJailerAllyMaster_prefab);
        private static GameObject _jailerAllyMasterPrefab;
        private static AssetReferenceT<GameObject> _jailerAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidJailer.VoidJailerAllyBody_prefab);
        //private static GameObject _jailerAllyBodyPrefab;

        private static AssetReferenceT<GameObject> _devastatorAllyMasterPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyMaster_prefab);
        private static GameObject _devastatorAllyMasterPrefab;
        private static AssetReferenceT<GameObject> _devastatorAllyBodyPrefabReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_VoidMegaCrab.VoidMegaCrabAllyBody_prefab);
        //private static GameObject _devastatorAllyBodyPrefab;
        private static readonly FixedConditionalWeakTable<CharacterMaster, LieVoidDiosInfo> _lieVoidDiosTable = [];
        private class LieVoidDiosInfo
        {
            internal GameObject OriginalBodyPrefab;
            internal string NameOfSceneFirstRevivedIn;
        }



        internal static void Setup()
        {
            if (!ConfigOptions.EnableVoidDiosEdit.Value)
            {
                return;
            }

            AssignMasterPrefabs();
            EditVoidBodyPrefabs();
            EditItemPrefabs();
            IL.RoR2.Items.ExtraLifeVoidManager.Init += ExtraLifeVoidManager_Init;
            IL.RoR2.CharacterMaster.RespawnExtraLifeVoid += CharacterMaster_RespawnExtraLifeVoid;
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
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



        private static void ExtraLifeVoidManager_Init(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchStsfld("RoR2.Items.ExtraLifeVoidManager", "voidBodyNames")
            ))
            {
                ModUtil.LogILError(il, c);
                return;
            }

            // i could surgically replace/insert strings when the array's created but i can't be bothered to do that
            c.EmitDelegate<Action>(() =>
            {
                ExtraLifeVoidManager.voidBodyNames = ConfigOptions.AllowRespawnAsVoidReaver.Value ?
                ["NullifierAllyBody", "VoidJailerAllyBody", "VoidMegaCrabAllyBody"] : ["VoidJailerAllyBody", "VoidMegaCrabAllyBody"];
            });
        }

        private static void CharacterMaster_RespawnExtraLifeVoid(ILContext il)
        {
            ILCursor c = new(il);

            // go before respawn line
            if (!c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdarg(0),
                x => x.MatchLdloc(0),
                x => x.MatchLdcR4(0)
            ))
            {
                ModUtil.LogILError(il, c);
                return;
            }
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CharacterMaster>>((characterMaster) =>
            {
                if (!_lieVoidDiosTable.TryGetValue(characterMaster, out _))
                {
                    _lieVoidDiosTable.Add(characterMaster, new LieVoidDiosInfo { OriginalBodyPrefab = characterMaster.bodyPrefab, NameOfSceneFirstRevivedIn = SceneManager.GetActiveScene().name });
                    Log.Info($"Giving {characterMaster.GetBody()?.name} a CutHp due to them using up a void dios for the first time this stage");
                    characterMaster.inventory?.GiveItem(_cutHpItemDef.itemIndex);
                }

                GameObject voidAllyBodyPrefab = ExtraLifeVoidManager.GetNextBodyPrefab(); // the method to pick a void guy to respawn as is still there, just unused
                characterMaster.bodyPrefab = voidAllyBodyPrefab;

                BaseAI originalBaseAI = characterMaster.GetComponent<BaseAI>();
                GameObject voidAllyMasterPrefab = GetVoidAllyMasterPrefabFromBodyPrefab(voidAllyBodyPrefab);
                if (originalBaseAI != null && voidAllyMasterPrefab != null)
                {
                    ReplaceAISkillDrivers(characterMaster, originalBaseAI, voidAllyMasterPrefab); // fixes the ai being lobotomized since the ai doesn't change with their body
                }
            });




            ILLabel skipItemVoiding = c.DefineLabel();

            // go before foreach line
            if (!c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdloca(4),
                x => x.MatchCall(out _),
                x => x.MatchStloc(5)
            ))
            {
                ModUtil.LogILError(il, c);
                return;
            }
            c.Emit(OpCodes.Br, skipItemVoiding);

            // go to the end of the foreach block
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<IDisposable>("Dispose"),
                x => x.MatchEndfinally()
            ))
            {
                ModUtil.LogILError(il, c);
                return;
            }
            c.MarkLabel(skipItemVoiding);
        }

        #region separate CharacterBody_Start region because collapsing it collapses everything below it for some reason
        private static void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            if (self == null || self.master == null)
            {
                orig(self);
                return;
            }
            if (!_lieVoidDiosTable.TryGetValue(self.master, out var lieVoidDiosInfo))
            {
                orig(self);
                return;
            }
            if (lieVoidDiosInfo.NameOfSceneFirstRevivedIn == SceneManager.GetActiveScene().name)
            {
                orig(self);
                return;
            }



            self.master.bodyPrefab = lieVoidDiosInfo.OriginalBodyPrefab;
            _lieVoidDiosTable.Remove(self.master);
            if (self.master.inventory != null)
            {
                if (self.master.inventory.GetItemCount(_cutHpItemDef.itemIndex) > 0)
                {
                    Log.Info($"Removing a CutHp from {self.name} that is Start-ing in a new stage after being revived by a void dios on the previous stage");
                    self.master.inventory.RemoveItem(_cutHpItemDef.itemIndex);
                }
            }
            else
            {
                Log.Error("SELF MASTER INVENTORY IN CharacterBody_Start WAS NULL!!!!! NOT GOOOD!!!!! YOU'RE STUCK WITH A CUTHP ITEM NOW!!!");
            }


            // HACK: for some reason the 3rd if statement in this method causes the bodyPrefab change to happen too late
            // so we'll respawn the body and not call orig here
            // that way the Start will happen only once and with the proper body

            // this still makes you spawn in the air sometimes but idc it's good enough
            Vector3 newSpawnPosition = TeleportHelper.FindSafeTeleportDestination(self.corePosition, self.master.bodyPrefab.GetComponent<CharacterBody>(), RoR2Application.rng) ?? self.corePosition;
            self.master.Respawn(newSpawnPosition, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f), true);
            return;
        }
        #endregion



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
}