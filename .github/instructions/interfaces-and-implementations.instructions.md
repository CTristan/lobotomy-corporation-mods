---
description: "Use when adding or changing interfaces and concrete classes across projects. Enforces folder layout, naming, visibility, and abstraction patterns."
applyTo: "**/*.cs"
---

# Interface And Implementation Conventions

Apply these rules across projects in this repository.

## Folder And Namespace Layout

- Place interfaces under an `Interfaces/` folder.
- Place concrete classes under an `Implementations/` folder.
- Match namespaces to folder segments:
  - `...Interfaces`
  - `...Implementations`
  - nested folders become nested namespaces (for example `Interfaces.Adapters.BaseClasses`, `Implementations.CreatureEvaluators`).

## Naming

- Interface names must use the `I` prefix (for example `ILogger`, `IAgentWorkTracker`, `ICreatureEvaluator`).
- Implementation names should use the same root name without `I` (for example `Logger : ILogger`, `AgentWorkTracker : IAgentWorkTracker`).
- Adapter interfaces and implementations should keep explicit domain names (for example `ITextTestAdapter` and `TextTestAdapter`).

## Visibility

- Use `public` for shared contracts and implementations in `LobotomyCorporationMods.Common`.
- Use `internal` for mod-private contracts and implementations that should not leak outside a specific mod assembly.
- Prefer `sealed` for concrete leaf implementations.
- Keep non-sealed classes only when they are intentional base classes (for example generic adapter base classes or abstract evaluator bases).

## Inheritance And Composition Patterns

- Use interfaces for seams that are injected, mocked, or swapped at runtime.
- For related families, define a small interface plus optional abstract base class:
  - example pattern: `ICreatureEvaluator` + `CreatureEvaluator` + sealed evaluator types.
- For adapter hierarchies, use generic base interfaces and base classes to share behavior:
  - example pattern: `ITestAdapter<T>` -> `IComponentTestAdapter<T>` and `TestAdapter<T>` -> `ComponentTestAdapter<T>`.
- Keep parameter/data-holder classes as plain concrete types unless a real polymorphic need exists.

## Dependency Usage

- Prefer interface-typed fields/properties/parameters at composition boundaries.
- You may internally instantiate a concrete class only if that class is itself Category 1 (fully testable — primitives and interfaces only in its constructor). Never internally instantiate a Category 2 boundary wrapper; those must always enter via an interface parameter.
- In patch methods and extension methods, accept interface parameters for collaborators that may be substituted in tests.

## Exceptions You Can Follow Intentionally

- A concrete class may implement an interface directly instead of inheriting the base class when acting as a sentinel or no-op implementation (for example `NoneEvaluator : ICreatureEvaluator`).
- Intermediate base classes are allowed to be non-sealed when they exist only to support inheritance for concrete sealed leaf types.
