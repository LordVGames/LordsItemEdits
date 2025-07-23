using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Orbs;
using RoR2BepInExPack.Utilities;
using MiscFixes.Modules;
using EntityStates.Drone.DroneWeapon;
using LordsItemEdits.MultiItemEdits;

namespace LordsItemEdits.ItemEdits
{
    internal class ATG
    {
        internal static void Setup()
        {
            if (!ConfigOptions.ATG.EnableAtgEdit.Value)
            {
                return;
            }

            IL.RoR2.GlobalEventManager.ProcessHitEnemy += GlobalEventManager_ProcessHitEnemy;
        }

        // inserting our own fire missile method and jumping over ATG's original one
        private static void GlobalEventManager_ProcessHitEnemy(ILContext il)
        {
            ILCursor c = new(il);



            // preliminary check to do anything else
            /*if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdcR4(3),
                x => x.MatchLdloc(out _),
                x => x.MatchConvR4(),
                x => x.MatchMul(),
                x => x.MatchStloc(out _)
            ))
            {
                Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 1");
                Log.Warning($"il is {il}");
                return;
            }*/
            // grabbing the missile damage index so that i'm not calling a specific number that'll change after every update
            int atgDamageLocIndex = 0;
            if (!c.TryGotoNext(MoveType.After,
                //x => x.MatchLdarg(1),
                x => x.MatchLdfld<DamageInfo>("damage"),
                x => x.MatchLdloc(0),
                x => x.MatchCallvirt(out _),
                x => x.MatchLdloc(out _),
                x => x.MatchCall("RoR2.Util", "OnHitProcDamage"),
                x => x.MatchStloc(out atgDamageLocIndex)
            ))
            {
                Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 2");
                Log.Warning($"il is {il}");
                return;
            }
            // going to before the firemissile line
            if (!c.TryGotoNext(MoveType.Before,
                x => x.MatchLdloc(0),
                x => x.MatchCallOrCallvirt(out _),
                x => x.MatchLdloc(0),
                x => x.MatchLdarg(1),
                x => x.MatchLdfld(out _)
            ))
            {
                Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 3");
                Log.Warning($"il is {il}");
                return;
            }



            c.Emit(OpCodes.Ldloc, 0);
            c.Emit(OpCodes.Ldloc, atgDamageLocIndex);
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldarg_2);
            c.EmitDelegate<Action<CharacterBody, float, DamageInfo, GameObject>>((attackerBody, missileDamage, damageInfo, victim) =>
            {
                Missiles.FireMissileOrb(attackerBody, missileDamage, damageInfo, victim);
            });



            ILLabel skipAtgFireMissile = c.DefineLabel();
            c.Emit(OpCodes.Ldloc, 0);
            c.EmitDelegate<Func<CharacterBody, bool>>((attackerBody) =>
            {
                return attackerBody.teamComponent.teamIndex == TeamIndex.Player;
            });
            c.Emit(OpCodes.Brtrue, skipAtgFireMissile);



            // going after firemissile line
            if (!c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld(out _),
                x => x.MatchLdcI4(3),
                x => x.MatchLdcI4(1),
                x => x.MatchCall(out _)
            ))
            {
                Log.Error($"COULD NOT IL HOOK {il.Method.Name} PART 4");
                Log.Warning($"il is {il}");
                return;
            }
            c.MarkLabel(skipAtgFireMissile);
        }
    }
}