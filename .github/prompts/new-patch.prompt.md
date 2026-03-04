---
description: "Scaffold a Harmony patch file and matching test file for a game class method"
agent: "agent"
---

Create a new Harmony patch for `{{ClassName}}.{{MethodName}}` in the `LobotomyCorporationMods.{{ModName}}` mod.

## Patch file

Create `LobotomyCorporationMods.{{ModName}}/Patches/{{ClassName}}Patch{{MethodName}}.cs` following the two-method pattern:

```csharp
// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.{{ModName}}.Patches
{
    [HarmonyPatch(typeof({{ClassName}}), nameof({{ClassName}}.{{MethodName}}))]
    public static class {{ClassName}}Patch{{MethodName}}
    {
        // Testable business logic as an extension method
        public static void PatchAfter{{MethodName}}([NotNull] this {{ClassName}} instance)
        {
            Guard.Against.Null(instance, nameof(instance));

            // TODO: Implement patch logic
        }

        // ReSharper disable InconsistentNaming
        [EntryPoint]
        [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
        public static void Postfix([NotNull] {{ClassName}} __instance)
        {
            try
            {
                __instance.PatchAfter{{MethodName}}();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);

                throw;
            }
        }
    }
}
```

Use `Prefix`/`PatchBefore` instead of `Postfix`/`PatchAfter` if the patch needs to run before the original method. Check `external/decompiled/` for the game class source to understand the method signature and determine:
- Whether to use Prefix or Postfix
- What parameters are available (`__instance`, `__result`, method arguments)
- What the return type is (adjust accordingly if the extension method needs to return a value)

## Test file

Create `LobotomyCorporationMods.Test/ModTests/{{ModName}}Tests/PatchTests/{{ClassName}}Patch{{MethodName}}Tests.cs`:

```csharp
// SPDX-License-Identifier: MIT

#region

using FluentAssertions;
using LobotomyCorporationMods.{{ModName}}.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.{{ModName}}Tests.PatchTests
{
    public sealed class {{ClassName}}Patch{{MethodName}}Tests : {{ModName}}ModTests
    {
        [Fact]
        public void TODO_describe_expected_behavior()
        {
            // Arrange
            // TODO: Create test objects using UnityTestExtensions

            // Act
            // TODO: Call the testable extension method

            // Assert
            // TODO: Verify behavior with FluentAssertions
        }
    }
}
```

## HarmonyPatchTests updates

Add these two tests to `LobotomyCorporationMods.Test/ModTests/{{ModName}}Tests/HarmonyPatchTests.cs`:

```csharp
[Fact]
public void Class_{{ClassName}}_Method_{{MethodName}}_is_patched_correctly()
{
    var patch = typeof({{ClassName}}Patch{{MethodName}});
    var originalClass = typeof({{ClassName}});
    const string MethodName = nameof({{ClassName}}.{{MethodName}});

    patch.ValidateHarmonyPatch(originalClass, MethodName);
}

[Fact]
public void Class_{{ClassName}}_Method_{{MethodName}}_logs_exceptions()
{
    var mockLogger = TestExtensions.GetMockLogger();
    Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);

    void Action()
    {
        {{ClassName}}Patch{{MethodName}}.Postfix(null);
    }

    mockLogger.VerifyArgumentNullException(Action);
}
```

After creating the files, run `dotnet build LobotomyCorporationMods.sln` to verify compilation.
