using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using HlLib;
using HlLib.CommandLine;
using HlLib.IO;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;

namespace Csi2
{
    class CommandOptions
    {
        [CommandLineArg(name = "debug")]
        public bool IsDebug { get; set; }

        [CommandLineArg(name = "define")]
        public string[] Defines { get; set; }

        [CommandLineArg(name = "lib")]
        public string[] LibraryRoots { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var optionsDef = new CommandLineOptions<CommandOptions>();

            string[] restArgs;
            var options = optionsDef.Parse(args, out restArgs);

            if (restArgs.Length == 0)
            {
                Console.Error.WriteLine("スクリプトファイルを指定してください");
                Environment.Exit(1);
            }

            var scriptSource = Path.GetFullPath(restArgs[0]);
            var scriptArgs = (restArgs.Length > 1) ? restArgs.Slice(1) : Array.Empty<string>();

#if false
            var scriptOptions = ScriptOptions.Default
                .WithReferences(Assembly.GetExecutingAssembly())
                .WithFilePath(scriptSource)
                .WithSourceResolver(
                    ScriptSourceResolver.Default
                        .WithSearchPaths(
                            options.LibraryRoots.Select(path => Environment.ExpandEnvironmentVariables(path))));
            Script script;
            using (var reader = new StreamReader(scriptSource))
            {
                script = CSharpScript.Create(reader.ReadToEnd(), scriptOptions);
            }

            ScriptContext.ScriptPath = scriptSource;
            ScriptContext.CommandLineArgs = scriptArgs;

            var diags = script.Compile();
            if (diags.Length > 0)
            {
                foreach (var diag in diags)
                {
                    Console.Error.WriteLine(diag);
                }
                Environment.Exit(1);
            }

            var result = script.RunAsync().Result;
            Console.WriteLine(result.ReturnValue);
#elif true
            var scriptPath = Path.GetFullPath("TestScript.csx");

            var compilationOption = new CSharpCompilationOptions(
                OutputKind.ConsoleApplication,
                optimizationLevel: OptimizationLevel.Release,
                sourceReferenceResolver: ScriptSourceResolver.Default
                    .WithSearchPaths(Environment.CurrentDirectory),
                metadataReferenceResolver: ScriptPathResolver.Default
                    .WithSearchPaths(
                        Environment.CurrentDirectory,
                        RuntimeEnvironment.GetRuntimeDirectory()));

            string text;
            using (var reader = new StreamReader(scriptPath))
            {
                text = reader.ReadToEnd();
            }
            var tree = SyntaxFactory.ParseSyntaxTree(
                text,
                new CSharpParseOptions(
                    kind: SourceCodeKind.Script,
                    documentationMode: DocumentationMode.None),
                scriptPath,
                Encoding.UTF8);

            var references = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(name => MetadataReference.CreateFromFile(Assembly.ReflectionOnlyLoad(name.FullName).Location));

            var compilation = CSharpCompilation.Create(
                "test", new[] { tree }, references, compilationOption);

            var emitResult = compilation.Emit("test.exe", "test.pdb");
            if (!emitResult.Success)
            {
                foreach (var diag in emitResult.Diagnostics)
                {
                    Console.WriteLine(diag);
                }
            }
#else
            var sourceText = new StringBuilder();
            using (var reader = new StreamReader("TestScript.csx"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();
                    if (line.StartsWith("#load"))
                    {
                    }
                    else if (line.StartsWith("#r"))
                    {
                    }
                    else
                    {
                        sourceText.AppendLine(line);
                    }
                }
            }

            var provider = new CSharpCodeProvider();
            var options = new CompilerParameters()
            {
                GenerateExecutable = true,
                OutputAssembly = "test.exe",
                CompilerOptions = "/define:hogehoge",
            };
            var result = provider.CompileAssemblyFromSource(options, sourceText.ToString());
            if (result.Output.Count > 0)
            {
                foreach (var output in result.Output)
                {
                    Console.WriteLine(output);
                }
            }
            if (result.Errors.Count > 0)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error);
                }
            }
#endif
                Console.Read();
        }
    }
}
