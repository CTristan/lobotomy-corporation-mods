# Use .NET Framework 3.5 for Lobotomy Corporation Mods

## Context and Problem Statement

Lobotomy Corporation uses Unity 2017.4, which is built on .NET Framework 3.5. When creating mods, we must choose a
compatible framework. Using a newer version (e.g., .NET Framework 4.0) may enable additional language and library
features during development but introduces binary incompatibilities at runtime.

This decision directly affects:

- Compatibility with the base game
- Runtime stability
- Developer experience in IDEs

## Considered Options

- .NET Framework 3.5
- .NET Framework 4.0

## Decision Outcome

### Chosen Option: .NET Framework 3.5

The base game uses .NET Framework 3.5, and mods must match this version to avoid runtime issues. While code built
against .NET Framework 4.0 may compile and appear to function, using them can result in subtle runtime crashes or
unexpected behavior due to IL incompatibilities.

### Rejected Option: .NET Framework 4.0

Using .NET Framework 4.0 introduces several incompatibility risks:

- LINQ extension resolution: Some LINQ methods are available in .NET Framework 4.0 but not in 3.5. For example,
  `List<T>.Select()`
  is an extension defined in `System.Core` for `IEnumerable<T>` — not a direct method on `List<T>`. In .NET Framework
  4.0, additional extension methods may be offered by the IDE that do not exist at runtime in the game’s environment.
- Namespace resolution differences: Extension methods may resolve to different overloads or be implicitly available via
  different assemblies. These differences can cause methods to appear usable in the IDE, but fail silently or throw
  `MissingMethodException` when run inside the game.
- Threading behavior: Although the C# `lock` keyword compiles to the same IL instruction (`Monitor.Enter`/`Exit`),
  differences in the BCL (Base Class Library) implementation between .NET Framework 3.5 and 4.0 may lead to issues if
  underlying methods or types are missing or incompatible. For more information, see this discussion on `lock` and
  `Monitor.Enter`: https://stackoverflow.com/questions/2837070/lock-statement-vs-monitor-enter-method/2837224#2837224

### Consequences

* Good, because we ensure compatibility with the game's runtime environment.
* Good, because developers won't be misled by IDE-suggested APIs that don't exist at runtime.
* Bad, because this limits access to newer language and library features.
* Bad, because most modern libraries no longer support .NET Framework 3.5.
