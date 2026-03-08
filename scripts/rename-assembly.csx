// SPDX-License-Identifier: MIT
// Simple script to rename an assembly using Mono.Cecil

using System;
using System.IO;
using Mono.Cecil;

var sourceDll = args[0];
var targetAssemblyName = args[1];

if (!File.Exists(sourceDll))
{
    Console.Error.WriteLine($"File not found: {sourceDll}");
    Environment.Exit(1);
}

Console.WriteLine($"Renaming assembly in {sourceDll} to {targetAssemblyName}");

using var assembly = AssemblyDefinition.ReadAssembly(sourceDll);
assembly.Name.Name = targetAssemblyName;

var tempPath = sourceDll + ".tmp";
assembly.Write(tempPath);
File.Delete(sourceDll);
File.Move(tempPath, sourceDll);

Console.WriteLine("Done!");
