---
description: "Use when suppressing analyzer warnings or diagnostics. Enforces .editorconfig as the single source of truth for rule exclusions — SuppressMessage must never be used when .editorconfig can handle the suppression."
applyTo: "**/*.cs"
---

# Warning Suppression Rules

`.editorconfig` is the **single source of truth** for analyzer rule exclusions.

## Rule

**Never use `[SuppressMessage]`** when the suppression can be expressed in `.editorconfig`. This applies to all CA, IDE, and RCS diagnostic codes.

Instead, add a `dotnet_diagnostic.{ID}.severity = none` entry under the appropriate file glob pattern in `.editorconfig`, with a `# WHY:` comment explaining the reason.

### Correct — `.editorconfig` with scoped glob

```ini
# WHY: JsonUtility data classes require public lowercase fields and case-differing accessors
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

## When `[SuppressMessage]` Is Acceptable

Only when **all** of the following are true:

1. The suppression applies to a **single specific member** (not a whole file or folder)
2. Other code in the same file glob **should** still trigger the rule
3. No `.editorconfig` glob can reasonably isolate the case

This is rare. If you find yourself reaching for `[SuppressMessage]`, first check whether a narrower `.editorconfig` glob (e.g., `[**/Protocol/**.cs]`) can scope the exclusion instead.

## Complementary Rules

- **csproj files**: Never use `<NoWarn>` for CA/IDE/RCS codes — see `csproj-conventions.instructions.md`
- **`.editorconfig` comments**: Every `dotnet_diagnostic` entry must have a `# WHY:` comment on the preceding line
