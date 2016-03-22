using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Csi2
{
    public static class ScriptContext
    {
        public static string[] CommandLineArgs { get; internal set; }
            = Array.Empty<string>();

        public static bool IsInMainFile([CallerFilePath] string filepath = null)
            => (ScriptPath != null && ScriptPath == filepath);

        internal static string ScriptPath { private get; set; }
    }
}
