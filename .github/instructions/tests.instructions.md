---
description: "Use when writing or modifying tests for Lobotomy Corporation mods. Covers xUnit, FluentAssertions, Moq patterns, and patch test structure."
applyTo: "LobotomyCorporationMods.Test/**"
---

# Test Conventions

## Structure

- Tests live in `ModTests/{ModName}Tests/` mirroring the mod's structure
- Each mod has a base class `{ModName}ModTests` with shared setup (instantiates `Harmony_Patch`, configures mock logger)
- Patch tests go in `PatchTests/{ClassName}Patch{MethodName}Tests.cs`
- `HarmonyPatchTests.cs` at the mod level verifies all patch registrations and exception logging

## Naming

Test methods are descriptive sentences with underscores:

```csharp
A_gift_that_has_not_been_worked_on_yet_displays_the_base_value
Does_not_attempt_to_update_newly_generated_agents
Changing_random_generated_agent_marks_them_as_custom
```

## Assertions

Use FluentAssertions — never raw `Assert`:

```csharp
result.Should().Be(expected);
result.Should().BeEquivalentTo(expected);
collection.Should().ContainValue(expected);
action.Should().Throw<ArgumentNullException>();
```

## Mocking

Use Moq for interfaces. Common patterns:

```csharp
var mockLogger = TestExtensions.GetMockLogger();
mockLogger.VerifyArgumentNullException(Action);
mockLogger.VerifyNullReferenceException(Action);
```

## Unity test objects

Use `UnityTestExtensions` factory methods — never `new` Unity types directly:

```csharp
UnityTestExtensions.CreateAgentModel()
UnityTestExtensions.CreateCreatureEquipmentMakeInfo()
UnityTestExtensions.CreateCustomizingWindow(currentAgent, currentWindowType)
UnityTestExtensions.CreateWorkerSpriteManager()
```

## Every patch must have

1. **Registration test** — `ValidateHarmonyPatch(originalClass, methodName)` in `HarmonyPatchTests.cs`
2. **Exception logging test** — call Postfix/Prefix with null, verify via `mockLogger.VerifyArgumentNullException`
3. **Business logic tests** — call the extension method directly (not the Postfix/Prefix)

## Pattern

```csharp
[Fact]
public void Descriptive_test_name()
{
    // Arrange
    var sut = InitializeGameObject();

    // Act
    var result = sut.PatchAfterMethod(args);

    // Assert
    result.Should().Be(expected);
}
```

## Coverage

100% patch coverage is enforced via Codecov. The `[ExcludeFromCodeCoverage]` Postfix/Prefix methods are excluded — test the extension methods instead.
