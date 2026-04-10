// SPDX-License-Identifier: MIT

// Custom entry point for xUnit v3. Auto-generation is disabled because
// LobotomyCorporation.Mods.Common (net35) defines a public ExcludeFromCodeCoverageAttribute
// polyfill that conflicts with the BCL type, causing CS0433 in auto-generated code.

#region

using System.Linq;

#endregion

namespace LobotomyCorporationMods.Test;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Any(arg => arg == "--server" || arg == "--internal-msbuild-node"))
        {
            return Xunit
                .MicrosoftTestingPlatform.TestPlatformTestFramework.RunAsync(
                    args,
                    AddSelfRegisteredExtensions
                )
                .GetAwaiter()
                .GetResult();
        }

        return Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args).GetAwaiter().GetResult();
    }

    private static void AddSelfRegisteredExtensions(
        Microsoft.Testing.Platform.Builder.ITestApplicationBuilder builder,
        string[] args
    )
    {
        Microsoft.Testing.Platform.MSBuild.TestingPlatformBuilderHook.AddExtensions(builder, args);
        Microsoft.Testing.Extensions.Telemetry.TestingPlatformBuilderHook.AddExtensions(
            builder,
            args
        );
    }
}
