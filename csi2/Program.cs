using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace csi2
{
    public static class ScriptUtil
    {
        public static bool IsInMainFile([CallerFilePath] string filepath = "")
        {
            return filepath == _entry.FullName;
        }

        internal static void SetEntryFile(string filepath)
        {
            _entry = new FileInfo(filepath);
        }

        private static FileInfo _entry;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = ScriptOptions.Default
                .WithReferences(Assembly.GetExecutingAssembly());

            var filepath = Path.GetFullPath("TestScript.csx");
            ScriptUtil.SetEntryFile(filepath);

            Script script = CSharpScript.Create(
                string.Format(@"#load ""{0}""", filepath),
                options);

            var diags = script.Compile();
            foreach (var diag in diags)
            {
                Console.WriteLine(diag);
            }

            if (diags.Length == 0)
            {
                script.RunAsync().Wait();
            }

            Console.Read();
        }
    }
}
