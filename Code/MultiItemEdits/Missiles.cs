using HG;
using LordsItemEdits.ItemEdits;
using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LordsItemEdits.MultiItemEdits
{
    internal class Missiles
    {
        private static readonly AssetReferenceT<GameObject> _microMissileProjectileAssetReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_Base_Drones.MicroMissileProjectile_prefab);
        private static readonly AssetReferenceT<GameObject> _microMissileOrbEffectReference = new(RoR2BepInExPack.GameAssetPaths.RoR2_DLC1_DroneWeapons.MicroMissileOrbEffect_prefab);


        internal static void Setup()
        {
            // why doesn't this have a start sound???
            /*AssetAsyncReferenceManager<GameObject>.LoadAsset(_microMissileProjectileAssetReference).Completed += (handle) =>
            {
                handle.Result.GetComponent<ProjectileController>().startSound = "Play_item_proc_missile_fire";
                AssetAsyncReferenceManager<GameObject>.UnloadAsset(_microMissileProjectileAssetReference);
            };
            AssetAsyncReferenceManager<GameObject>.LoadAsset(_microMissileOrbEffectReference).Completed += (handle) =>
            {
                handle.Result.GetComponent<EffectComponent>().soundName = "Play_item_proc_missile_fire";
                AssetAsyncReferenceManager<GameObject>.UnloadAsset(_microMissileOrbEffectReference);
            };*/
        }


        internal static void FireMissileOrb(CharacterBody attackerBody, float missileDamage, DamageInfo damageInfo, GameObject victim)
        {
            if (victim == null || attackerBody.teamComponent.teamIndex != TeamIndex.Player)
            {
                return;
            }
            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
            MicroMissileOrb missileOrb = new()
            {
                origin = attackerBody.aimOrigin,
                damageValue = missileDamage,
                isCrit = damageInfo.crit,
                teamIndex = attackerBody.teamComponent.teamIndex,
                attacker = attackerBody.gameObject,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Item,
                target = victimBody.mainHurtBox
            };
            missileOrb.procChainMask.AddProc(ProcType.Missile);
            missileOrb.damageValue *= PocketICBM.GetICBMDamageMult(attackerBody);
            OrbManager.instance.AddOrb(missileOrb);
            // the orb doesn't play a sound on fire and editing the assets isn't working so
            Util.PlaySound("Play_item_proc_missile_fire", attackerBody.gameObject);
        }
    }
}