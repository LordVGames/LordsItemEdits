using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Text;

namespace LordsItemEdits
{
    internal class ModUtil
    {
        internal static void LogILError(ILContext il, ILCursor c)
        {
            Log.Error($"COULD NOT IL HOOK {il.Method.Name}");
            Log.Warning($"cursor is {c}");
            Log.Warning($"il is {il}");
        }
    }
}