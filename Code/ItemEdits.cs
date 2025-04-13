using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Items;
using RoR2BepInExPack.Utilities;
using static UnityEngine.Object;
using RoR2.CharacterAI;
using UnityEngine.AddressableAssets;

namespace LordsItemEdits
{
    internal static class ItemEdits
    {
        internal static void DoEdits()
        {
            VoidDios.Setup();
        }



        private class VoidDios
        {
            private static GameObject _reaverAllyMasterPrefab;
            private static GameObject _jailerAllyMasterPrefab;
            private static GameObject _devastatorAllyMasterPrefab;
            private static readonly FixedConditionalWeakTable<CharacterMaster, LieVoidDiosInfo> _lieVoidDiosTable = new();
            private class LieVoidDiosInfo
            {
                internal GameObject OriginalBodyPrefab;
            }



            internal static void Setup()
            {
                if (!ConfigOptions.EnableVoidDiosEdit.Value)
                {
                    return;
                }

                GetVoidAllyMasterAssets();
                IL.RoR2.Items.ExtraLifeVoidManager.Init += ExtraLifeVoidManager_Init;
                IL.RoR2.CharacterMaster.RespawnExtraLifeVoid += CharacterMaster_RespawnExtraLifeVoid;
                On.RoR2.CharacterBody.Start += CharacterBody_Start;
            }

            private static void GetVoidAllyMasterAssets()
            {
                _reaverAllyMasterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierAllyMaster.prefab").WaitForCompletion();
                _jailerAllyMasterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidJailer/VoidJailerAllyMaster.prefab").WaitForCompletion();
                _devastatorAllyMasterPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabAllyMaster.prefab").WaitForCompletion();
            }

            private static void ExtraLifeVoidManager_Init(ILContext il)
            {
                ILCursor c = new(il);

                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchStsfld("RoR2.Items.ExtraLifeVoidManager", "voidBodyNames")
                ))
                {
                    LogILError(il, c);
                    return;
                }

                // i could surgically replace/insert strings when the array's created but i'm not doing that rn
                c.EmitDelegate<Action>(() =>
                {
                    if (ConfigOptions.AllowRespawnAsVoidReaver.Value)
                    {
                        ExtraLifeVoidManager.voidBodyNames = ["NullifierAllyBody", "VoidJailerAllyBody", "VoidMegaCrabAllyBody"];
                    }
                    else
                    {
                        ExtraLifeVoidManager.voidBodyNames = ["VoidJailerAllyBody", "VoidMegaCrabAllyBody"];
                    }
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
                    LogILError(il, c);
                    return;
                }
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<CharacterMaster>>((characterMaster) =>
                {
                    if (!_lieVoidDiosTable.TryGetValue(characterMaster, out _))
                    {
                        _lieVoidDiosTable.Add(characterMaster, new LieVoidDiosInfo { OriginalBodyPrefab = characterMaster.bodyPrefab });
                    }
                    BaseAI originalBaseAI = characterMaster.GetComponent<BaseAI>();
                    GameObject voidAllyBodyPrefab = ExtraLifeVoidManager.GetNextBodyPrefab(); // the method to pick a void guy to respawn as is still there, just unused
                    GameObject voidAllyMasterPrefab = GetVoidAllyMasterPrefabFromBodyPrefab(voidAllyBodyPrefab);


                    characterMaster.bodyPrefab = voidAllyBodyPrefab;
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
                    LogILError(il, c);
                    return;
                }
                c.Emit(OpCodes.Br, skipItemVoiding);

                // go to the end of the foreach block
                if (!c.TryGotoNext(MoveType.After,
                    x => x.MatchCallvirt<IDisposable>("Dispose"),
                    x => x.MatchEndfinally()
                ))
                {
                    LogILError(il, c);
                    return;
                }
                c.MarkLabel(skipItemVoiding);
            }

            #region separate CharacterBody_Start region because collapsing it collapses everything below it for some reason
            private static void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
            {
                if (self != null && self.master != null)
                {
                    if (_lieVoidDiosTable.TryGetValue(self.master, out var lieVoidDiosInfo))
                    {
                        self.master.bodyPrefab = lieVoidDiosInfo.OriginalBodyPrefab;
                        _lieVoidDiosTable.Remove(self.master);
                    }
                }
                orig(self);
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



        private static void LogILError(ILContext il, ILCursor c)
        {
            Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
            Log.Warning($"cursor is {c}");
            Log.Warning($"il is {il}");
        }
    }
}
