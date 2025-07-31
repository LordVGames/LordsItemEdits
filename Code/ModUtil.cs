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
        internal static string GetLangFileLocation(string fileName)
        {
            string langFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Plugin.PluginInfo.Location), "Language");
            return System.IO.Path.Combine(langFolderPath, (fileName + ".language"));
        }

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