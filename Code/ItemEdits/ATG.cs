using MiscFixes.Modules;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Orbs;
using RoR2BepInExPack.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using static UnityEngine.Object;

namespace LordsItemEdits.ItemEdits
{
    internal static class ATG
    {
        private static readonly ItemDef nullItemDef = null;
        private const float initialDamageCoefficient = 3f;
        //private const float stackDamageCoefficient = 3f;



        internal static void Setup()
        {
            if (!ConfigOptions.EnableAtgEdit.Value)
            {
                return;
            }

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }



        private static void GlobalEventManager_ProcessHitEnemy(ILContext il)
        {
            // based on/copied from riskymod's atg changes code
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(typeof(RoR2Content.Items), "Missile")
            ))
            {
                ModUtil.LogILError(il, c);
                return;
            }

            //c.Emit(OpCodes.Ldloc, 1); //victimBody
            c.Emit(OpCodes.Ldloc, 7); //master
            //c.Emit(OpCodes.Ldarg_1);    //damageinfo
            //c.EmitDelegate<Func<ItemDef, CharacterBody, CharacterMaster, DamageInfo, ItemDef>>((item, victimBody, master, damageInfo) =>
            c.EmitDelegate<Func<ItemDef, CharacterMaster, ItemDef>>((item, master) =>
            {
                if (master.teamIndex == TeamIndex.Player)
                {
                    item = nullItemDef;
                }
                return item;
            });
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            // based on/copied from riskymod's AtgOrb code
            if (!NetworkServer.active || damageInfo.procCoefficient == 0 || damageInfo.rejected)
            {
                orig(self, damageInfo, victim);
                return;
            }
            if (damageInfo.attacker == null)
            {
                orig(self, damageInfo, victim);
                return;
            }
            if (damageInfo.procChainMask.HasProc(ProcType.Missile))
            {
                orig(self, damageInfo, victim);
                return;
            }
            //Only players fire orbs. Enemies fire physical missiles so that you can evade them by using cover.
            //Based off of Plasma Shrimp code
            CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            if (attackerBody == null || attackerBody.inventory == null || attackerBody.aimOrigin == null || attackerBody.teamComponent == null || attackerBody.teamComponent.teamIndex != TeamIndex.Player)
            {
                orig(self, damageInfo, victim);
                return;
            }
            int atgCount = attackerBody.inventory.GetItemCount(RoR2Content.Items.Missile);
            if (atgCount == 0)
            {
                orig(self, damageInfo, victim);
                return;
            }
            if (!Util.CheckRoll(10f * damageInfo.procCoefficient, attackerBody.master))
            {
                orig(self, damageInfo, victim);
                return;
            }
            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
            if (victimBody == null || victimBody.mainHurtBox == null)
            {
                orig(self, damageInfo, victim);
                return;
            }



            // would use stackDamageCoefficient but it would just be the same as initialDamageCoefficient since i'm not changing ATG's stats (yet?)
            // so we're saving an itty bitty bit of memory or whatever yay
            float damageCoefficient = initialDamageCoefficient + initialDamageCoefficient * (atgCount - 1);
            float damageValue = Util.OnHitProcDamage(damageInfo.damage, attackerBody.damage, damageCoefficient);

            // can implement the icbm change for atg here since we're re-implementing atg rn anyways
            int icbmCount = attackerBody.inventory.GetItemCount(DLC1Content.Items.MoreMissile);
            if (icbmCount > 0)
            {
                damageValue *= PocketICBM.GetICBMDamageMult(icbmCount);
            }

            MicroMissileOrb missileOrb = new()
            {
                origin = attackerBody.aimOrigin,
                damageValue = damageValue,
                isCrit = damageInfo.crit,
                teamIndex = attackerBody.teamComponent.teamIndex,
                attacker = damageInfo.attacker,
                procChainMask = damageInfo.procChainMask,
                procCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Item,
                target = victimBody.mainHurtBox
            };
            missileOrb.procChainMask.AddProc(ProcType.Missile);
            OrbManager.instance.AddOrb(missileOrb);
        }
    }
}