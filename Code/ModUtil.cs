using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
namespace LordsItemEdits;


internal static class ModUtil
{
    internal static void LogILInstructions(this ILWeaver iLWeaver)
    {
        foreach (var instruction in iLWeaver.Instructions)
        {
            Log.Warning(instruction);
        }
    }
}