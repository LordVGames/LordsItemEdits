using MiscFixes;
using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.DetourTypes;
using MonoDetour.HookGen;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
namespace LordsItemEdits.ItemEdits;


[MonoDetourTargets(typeof(BoomerangProjectile))]
internal static class ElectricBoomerang
{
    private const float _damagePerHit = 3.75f;
    private const float _damagePerHitStack = 2.75f;
    private static readonly AssetReferenceT<GameObject> _electricBoomerangProjectileAsset = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_Items_StunAndPierce.StunAndPierceBoomerang_prefab);
    private static readonly AssetReferenceT<ItemDef> _electricBoomerangItemDef = new(RoR2BepInExPack.GameAssetPathsBetter.RoR2_DLC2_Items_StunAndPierce.StunAndPierce_asset);
    private static readonly FixedConditionalWeakTable<GameObject, LieElectricBoomerangInfo> _lieElectricBoomerangInfoTable = [];
    private class LieElectricBoomerangInfo
    {
        internal bool HasBoomerangOut;
    }



    [MonoDetourHookInitialize]
    internal static void Setup()
    {
        if (!ConfigOptions.ElectricBoomerang.EnableEdit.Value)
        {
            return;
        }

        ModLanguage.LangFilesToLoad.Add("ElectricBoomerang");
        EditBoomerangProjectile();
        AIBlacklistNewBoomerang();
        Mdh.RoR2.GlobalEventManager.ProcessHitEnemy.Postfix(GlobalEventManager_ProcessHitEnemy);
        Mdh.RoR2.GlobalEventManager.ProcessHitEnemy.ILHook(MakeBoomerangBetter);
        Mdh.RoR2.Projectile.BoomerangProjectile.FixedUpdate.ILHook(InsertRemoveProjectileOwnerFromFiredList);
    }

    private static void GlobalEventManager_ProcessHitEnemy(GlobalEventManager self, ref DamageInfo damageInfo, ref GameObject victim)
    {
        if (victim != null && damageInfo.inflictor != null && damageInfo.inflictor.name == "StunAndPierceBoomerang(Clone)")
        {
            // man i just want this sound to be heard, but it's quiet
            // so i'm just gonna spam it
            // afaik nothing bad happens from this but i don't like doing this
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
            Util.PlaySound("Play_item_proc_chain_lightning", victim);
        }
    }



    private static void EditBoomerangProjectile()
    {
        AssetAsyncReferenceManager<GameObject>.LoadAsset(_electricBoomerangProjectileAsset).Completed += (handle) =>
        {
            var boomerangProjectile = handle.Result.GetComponent<BoomerangProjectile>();
            boomerangProjectile.distanceMultiplier *= 0.15f;
            boomerangProjectile.travelSpeed *= 3;

            // this removes the bigger initial hit
            handle.Result.TryDestroyComponent<ProjectileOverlapAttack>();

            AssetAsyncReferenceManager<GameObject>.UnloadAsset(_electricBoomerangProjectileAsset);
        };
    }



    private static void AIBlacklistNewBoomerang()
    {
        AssetAsyncReferenceManager<ItemDef>.LoadAsset(_electricBoomerangItemDef).Completed += (handle) =>
        {
            // make it ai blacklisted since players can't be stunned
            ItemDef electricBoomerangItemDef = handle.Result;
            electricBoomerangItemDef.tags = [.. electricBoomerangItemDef.tags, ItemTag.AIBlacklist];
            AssetAsyncReferenceManager<ItemDef>.UnloadAsset(_electricBoomerangItemDef);
        };
    }



    private static void MakeBoomerangBetter(ILManipulationInfo info)
    {
        ILWeaver w = new(info);
        ILLabel skipElectricBoomerangCode = w.DefineLabel();
        ILLabel skipOldInitialCheck = w.DefineLabel();
        ILLabel skipOldDamageSet = w.DefineLabel();
        int electricBoomerangEffectiveCountVariableNumber = 0;
        int setDamageVariableNumber = 0;

        // going before:
        // if (itemCountEffective20 > 0 && !damageInfo.procChainMask.HasProc(ProcType.StunAndPierceDamage) && (ulong)(damageInfo.damageType & DamageTypeExtended.Electrocution) == 0L && LocalCheckRoll(15f * damageInfo.procCoefficient, master2))
        w.MatchRelaxed(
            x => x.MatchLdsfld("RoR2.DLC2Content/Items", "StunAndPierce"),
            x => x.MatchCallOrCallvirt<Inventory>("GetItemCountEffective"),
            x => x.MatchStloc(out _),
            x => x.MatchLdloc(out electricBoomerangEffectiveCountVariableNumber) && w.SetCurrentTo(x),
            x => x.MatchLdcI4(0),
            x => x.MatchBle(out skipElectricBoomerangCode)
        ).ThrowIfFailure()
        .InsertBeforeCurrent(
            w.Create(OpCodes.Ldarg_1), // load damageinfo
            w.Create(OpCodes.Ldloc, 24), // load electric boomerang count
            w.CreateCall(ShouldAllowNewElectricBoomerang),
            w.Create(OpCodes.Brfalse, skipElectricBoomerangCode),
            w.Create(OpCodes.Br, skipOldInitialCheck)
        );


        // going to end of line:
        // float damage6 = characterBody.damage * 0.4f * (float)itemCountEffective20;
        w.MatchRelaxed(
            x => x.MatchLdcR4(15),
            x => x.MatchLdarg(1),
            x => x.MatchLdfld<DamageInfo>("procCoefficient"),
            x => x.MatchMul(),
            x => x.MatchLdloc(out _),
            x => x.MatchLdloca(out _),
            x => x.MatchCallOrCallvirt(out _),
            x => x.MatchBrfalse(out _) && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .MarkLabelToCurrentNext(skipOldInitialCheck)
        .CurrentToNext()
        .InsertBeforeCurrentStealLabels(
            w.Create(OpCodes.Ldarg_1), // load damageinfo
            w.Create(OpCodes.Ldloc, electricBoomerangEffectiveCountVariableNumber), // load electric boomerang count
            w.CreateCall(GetNewDamageValue),
            w.Create(OpCodes.Stloc, setDamageVariableNumber),
            w.Create(OpCodes.Ldloc_0), // load characterbody
            w.CreateCall(TryAddCharacterBodyToFiredBoomerangsList),
            w.Create(OpCodes.Br, skipOldDamageSet)
        );


        // going to end of line above
        w.MatchRelaxed(
            x => x.MatchLdloc(27),
            x => x.MatchConvR4(),
            x => x.MatchMul(),
            x => x.MatchStloc(out setDamageVariableNumber) && w.SetCurrentTo(x)
        ).ThrowIfFailure()
        .MarkLabelToCurrentNext(skipOldDamageSet);


        //w.LogILInstructions();
    }

    private static bool ShouldAllowNewElectricBoomerang(DamageInfo damageInfo, int electricBoomerangCount)
    {
        return electricBoomerangCount < 1 && !damageInfo.procChainMask.HasProc(ProcType.StunAndPierceDamage) && damageInfo.damageType.IsDamageSourceSkillBased && damageInfo.damageType.damageType.HasFlag(DamageType.Stun1s) && !_lieElectricBoomerangInfoTable.ContainsKey(damageInfo.attacker);
    }

    private static float GetNewDamageValue(DamageInfo damageInfo, int electricBoomerangCount)
    {
        return damageInfo.damage * (_damagePerHit + (_damagePerHitStack * (electricBoomerangCount - 1)));
    }

    private static void TryAddCharacterBodyToFiredBoomerangsList(CharacterBody characterBody)
    {
        if (NetworkServer.active && !_lieElectricBoomerangInfoTable.ContainsKey(characterBody.gameObject))
        {
            _lieElectricBoomerangInfoTable.Add(characterBody.gameObject, new LieElectricBoomerangInfo { HasBoomerangOut = true });
        }
    }



    private static void InsertRemoveProjectileOwnerFromFiredList(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        w.MatchMultipleRelaxed(
            onMatch: mW =>
            {
                mW.InsertAfterCurrent(
                    mW.Create(OpCodes.Ldarg_0),
                    mW.CreateCall(TryRemoveProjectileOwnerFromFiredList)
                );
            },
            x => x.MatchCall("UnityEngine.Object", "Destroy") && w.SetCurrentTo(x)
        )
        .ThrowIfFailure();
    }
    
    private static void TryRemoveProjectileOwnerFromFiredList(BoomerangProjectile boomerangProjectile)
    {
        if (NetworkServer.active && _lieElectricBoomerangInfoTable.ContainsKey(boomerangProjectile.projectileController.owner))
        {
            _lieElectricBoomerangInfoTable.Remove(boomerangProjectile.projectileController.owner);
        }
    }
}