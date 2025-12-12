using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoDetour.Cil.Analysis;
using MonoMod.Cil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
namespace LordsItemEdits;


internal static class ModUtil
{
    internal static void LogILInstructions(this ILWeaver w)
    {
        Log.Warning(w.Method.Body.CreateInformationalSnapshotJIT().ToStringWithAnnotations());
    }


    internal static ILWeaverResult MatchNextRelaxed(this ILWeaver w, params Predicate<Instruction>[] predicates)
    {
        bool foundNextMatch = false;
        int oldWeaverOffset = w.Current.Offset;
        Instruction instructionToStayOn = null;

        ILWeaverResult matchResult = w.MatchMultipleRelaxed(
            onMatch: w2 =>
            {
                //Log.Debug($"w.Current.Offset {w.Current.Offset}");
                //Log.Debug($"w2.Current.Offset {w2.Current.Offset}");
                //Log.Debug($"w.Current {w.Current}");
                //Log.Debug($"w2.Current {w2.Current}");
                if (w2.Current.Offset > oldWeaverOffset && !foundNextMatch)
                {
                    //Log.Debug("FOUND");
                    foundNextMatch = true;
                    instructionToStayOn = w2.Current;
                }
            },
            predicates
        );
        if (!foundNextMatch)
        {
            return new ILWeaverResult(w, w.GetMatchToNextRelaxedErrorMessage);
        }

        w.SetCurrentTo(instructionToStayOn); // idk, just in case
        return matchResult;
    }
    private static string GetMatchToNextRelaxedErrorMessage(this ILWeaver w)
    {
        // this is stupid (i think?)
        StringBuilder sb = new();
        sb.Append(w.Method.Body.CreateInformationalSnapshotJIT().ToStringWithAnnotations());
        sb.AppendFormat($"\nLast Weaver Position: {w.Current}");
        sb.AppendFormat($"\nPrevious: {w.Previous}");
        sb.AppendFormat($"\nNext: {w.Next}");
        sb.AppendLine("\n\n! MatchNextRelaxed FAILED !\nA match was found, but it was not further ahead than the weaver's position!");
        return sb.ToString();
    }


    internal static void SkipNext2FireProjectiles(ILWeaver w)
    {
        ILLabel skipOverBad = w.DefineLabel();


        // go to start of first one
        ILWeaverResult firstMatch = w.MatchNextRelaxed(
            x => x.MatchCallOrCallvirt<ProjectileManager>("get_instance") && w.SetCurrentTo(x)
        );
        w.InsertBeforeCurrent(
            w.Create(OpCodes.Br, skipOverBad)
        );


        // go to start of second one to position for end of second one
        w.MatchNextRelaxed(
            x => x.MatchCallOrCallvirt<ProjectileManager>("get_instance") && w.SetCurrentTo(x)
        ).ThrowIfFailure();


        // go to end of second one
        ILWeaverResult match = w.MatchNextRelaxed(
            x => x.MatchCallOrCallvirt<ProjectileManager>("FireProjectile") && w.SetCurrentTo(x)
        );
        if (!match.IsValid)
        {
            Log.Warning("NOT VALID????");
            w.MatchNextRelaxed(
                x => x.MatchCallOrCallvirt<ProjectileManager>("FireProjectileWithoutDamageType") && w.SetCurrentTo(x)
            ).ThrowIfFailure();
        }

        
        w.MarkLabelToCurrentNext(skipOverBad);
    }
}