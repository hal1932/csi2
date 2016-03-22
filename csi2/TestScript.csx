#load "TestScript1.csx"
#r "csi2.exe"
#r "HlLib.dll"
#r "System.Net.Http.dll"
using Csi2;
using HlLib;
using System;
using System.Net.Http;

Console.WriteLine(System.AppDomain.CurrentDomain.Id);

[System.Diagnostics.Conditional("DEBUG")]
void f()
{
    Console.WriteLine("debug");
}

if (ScriptContext.IsInMainFile())
{
    Console.WriteLine("from TestScript.csx");
    f();
}

Console.WriteLine(new string[] { "aaa" }.Slice(0)[0]);

var client = new HttpClient();
var html = await client.GetStringAsync("http://google.com");
Console.WriteLine(html);

foreach (var arg in ScriptContext.CommandLineArgs)
{
    Console.WriteLine(arg);
}

return 0;
