---
description: "Use when creating or modifying constructors. Enforces the binary rule: every class is either fully testable (primitives/interfaces only) or a boundary wrapper (concrete types, ExcludeFromCodeCoverage, minimal logic)."
applyTo: "**/*.cs"
---

# Constructor Parameter Rules

Every class falls into exactly one of two categories. No exceptions.

## Category 1: Fully Testable Classes

Constructor parameters are exclusively **primitives** or **interfaces**.

Allowed primitive types: `string`, `int`, `long`, `float`, `double`, `decimal`, `bool`, `char`, `byte`, enum types (for example `RwbpType`), `Type`.

Allowed reference types: any interface (for example `ILogger`, `IFileManager`, `ICreatureEvaluator`).

These classes:
- Contain business logic
- Must have 100% code coverage
- Must NOT have `[ExcludeFromCodeCoverage]` on the class

## Category 2: Boundary Wrapper Classes

Constructor parameters include at least one **concrete Unity or game type** (for example `AgentModel`, `CreatureModel`, `GameObject`, `Component` subclasses, `DirectoryInfo`).

These classes:
- Must have `[AdapterClass]` and `[ExcludeFromCodeCoverage]` on the class
- Must contain **minimal logic** — only delegation, property forwarding, or trivial mapping
- Exist solely to bridge concrete types behind interfaces
- Push all business logic to Category 1 classes that accept the resulting interfaces

## Guiding Principle

Minimize the surface area of boundary wrappers. When business logic lives in a class that takes concrete types, refactor by:
1. Creating an interface that exposes only the needed properties and methods
2. Creating a thin boundary wrapper (Category 2) that adapts the concrete type to that interface
3. Changing the business-logic class (Category 1) to accept the interface instead
