using Mono.Cecil.Cil;
using MonoDetour.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits
{
    internal static class ModUtil
    {
        internal static void LogILError(ILContext il, ILCursor c)
        {
            Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
            Log.Warning($"cursor is {c}");
            Log.Warning($"il is {il}");
        }

        internal static void LogILInstructions(this ILWeaver iLWeaver)
        {
            foreach (var instruction in iLWeaver.Instructions)
            {
                Log.Warning(instruction);
            }
        }
    }
}