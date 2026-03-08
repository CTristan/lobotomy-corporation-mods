// SPDX-License-Identifier: MIT

// Entry point for the dotnet tool. The main logic is in Program class.
// This file exists because PackAsTool requires an entry point when OutputType is Library.

namespace LobotomyPlaywright;

public static class ToolEntryPoint
{
    public static int Main(string[] args) => Program.Main(args);
}
