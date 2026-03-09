---
description: "Use when suppressing analyzer warnings or diagnostics. Enforces .editorconfig as the single source of truth for rule exclusions — SuppressMessage and #pragma warning must never be used when .editorconfig can handle the suppression."
applyTo: "**/*.cs"
---

# Warning Suppression Rules

## First Priority: Fix the Warning

**Analyzer warnings exist to improve code quality.** When you encounter a warning, your **first and strongest obligation** is to fix the code so the warning no longer fires. Suppression of any kind is a **last resort**, not a convenience.

Before suppressing a warning, you must:

1. **Understand the rule** — read what the analyzer is flagging and why
2. **Fix the code** — rename, refactor, restructure, or redesign to satisfy the rule
3. **Only if the fix is impossible** (e.g., a framework constraint, Unity serialization requirement, or Harmony naming convention) should you suppress

If you suppress a warning, you must explain **why fixing the code is not possible** — not just what the warning is.

## When Suppression Is Necessary

`.editorconfig` is the **single source of truth** for analyzer rule exclusions. Never use `[SuppressMessage]`, `#pragma warning disable`, or `<NoWarn>` for analyzer diagnostics.

Add a `dotnet_diagnostic.{ID}.severity = none` entry under the appropriate file glob pattern in `.editorconfig`, with a `# WHY:` comment explaining **why the code cannot be fixed** to satisfy the rule.

### Correct — `.editorconfig` with scoped glob (when fix is impossible)

```ini
# WHY: JsonUtility data classes require public lowercase fields and case-differing accessors — Unity's
# serializer only works with public fields, and JSON keys must match the external schema
[**/JsonModels/**.cs]
dotnet_diagnostic.CA1051.severity = none
dotnet_diagnostic.IDE1006.severity = none
dotnet_diagnostic.CA1708.severity = none
```

The `JsonModels/` folder convention ensures all JsonUtility data classes are automatically covered (see `json-serialization.instructions.md`).

### Wrong — `[SuppressMessage]` on individual classes

```csharp
// DO NOT do this
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class GameStateData { ... }
```

### Wrong — `#pragma warning disable` for analyzer rules

```csharp
// DO NOT do this
#pragma warning disable CA1051
public string myField;
#pragma warning restore CA1051
```

## When `[SuppressMessage]` Is Acceptable

Only when **all** of the following are true:

1. The suppression applies to a **single specific member** (not a whole file or folder)
2. Other code in the same file glob **should** still trigger the rule
3. No `.editorconfig` glob can reasonably isolate the case

This is rare. If you find yourself reaching for `[SuppressMessage]`, first check whether a narrower `.editorconfig` glob (e.g., `[**/Protocol/**.cs]`) can scope the exclusion instead.

## When `#pragma warning disable` Is Acceptable

Only for **compiler warnings** (CS-prefixed codes) that `.editorconfig` cannot suppress. Analyzer diagnostics (CA, IDE, RCS) must always go in `.editorconfig`.

## Complementary Rules

- **csproj files**: Never use `<NoWarn>` for CA/IDE/RCS codes — see `csproj-conventions.instructions.md`
- **`.editorconfig` comments**: Every `dotnet_diagnostic` entry must have a `# WHY:` comment explaining why the code **cannot be fixed**, not just restating the rule name
