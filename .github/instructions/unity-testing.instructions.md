---
description: "Use when calling UnityEngine methods, writing tests, or fixing SecurityException / ECall errors. Enforces that Unity native methods (Debug.Log, Object.Instantiate) are never called directly in testable code."
applyTo: "**/*.cs"
---

# No Direct Unity Engine Calls in Testable Code

Unity types rely on native internal calls (`ECall`) that throw `System.Security.SecurityException` outside the Unity runtime — including in xUnit test hosts. This applies to:

- **Unity static methods**: `Debug.Log`, `Debug.LogError`, `Object.Instantiate`, etc.
- **Concrete game types that inherit from Unity types**: `AgentModel`, `CreatureModel`, `GameObject`, `Component` subclasses, and any other class rooted in `UnityEngine.Object`

Constructing, accessing, or calling methods on these types in test context will crash the test host with the same `ECall` error — even if your code never calls a `UnityEngine.*` method directly.

## Rule

**Never use Unity types or game types that inherit from Unity types in Category 1 (fully testable) classes.** Access them only through interfaces.

Direct Unity type usage belongs only in Category 2 boundary wrappers marked `[ExcludeFromCodeCoverage]`. See `constructors.instructions.md` for the Category 1/2 framework.

## Wrong

```csharp
// Category 1 class — this will crash in tests
public class ClientHandler
{
    public void HandleError(Exception ex)
    {
        UnityEngine.Debug.LogError($"Error: {ex.Message}");
    }
}
```

## Right

```csharp
// Category 1 class — testable, uses injected interface
public class ClientHandler
{
    private readonly ILogger _logger;

    public ClientHandler(ILogger logger)
    {
        _logger = logger;
    }

    public void HandleError(Exception ex)
    {
        _logger.WriteException(ex);
    }
}
```

The project provides `ILogger` and `ILoggerTarget` in `LobotomyCorporationMods.Common` for this purpose.
