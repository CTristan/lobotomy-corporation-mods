# Plan: Unified XML Localization for Tooling Projects

## Context

The mod projects use `Localize/{lang}/text_{lang}.xml` files for localization, loaded by the game's `LocalizeTextDataModel` with a `DefaultLocalizedValues` fallback in Common. Contributors already know this format.

The non-mod tooling projects (SetupExternal, CI) currently use hardcoded English string literals. The goal is to bring the same `Localize/{lang}/text_{lang}.xml` convention to these projects so that:

1. Community contributors can translate tools using the same workflow they already know from mods
2. Users can drop translation files next to the executable without rebuilding
3. English remains the built-in default

**Mod projects require no changes** — their XML system is dictated by the game and works well.

## Goals

- Same XML format and directory convention as mods: `Localize/{lang}/text_{lang}.xml`
- Tool loads locale files from next to the executable at runtime
- Falls back to English defaults when a key or locale file is missing
- String key constants class per project (like the existing `LocalizationIds` pattern)
- Testable: localization layer behind an interface

## Scope

| Project | Priority | Reason |
|---------|----------|--------|
| SetupExternal | Medium | ~73 strings, setup feedback |
| CI | Low/Skip | ~20 strings, developer-only |

Start with SetupExternal as the pilot. LobotomyPlaywright has been extracted to a standalone repo (`~/projects/lobotomy-playwright`).

## Approach

### Shared localization utility

Create a small localization class that can be used by any net10.0 project. Since there's no existing shared library for tools, options:

1. **New shared project** (e.g., `ToolCommon/`) — cleanest but adds a project
2. **Copy the utility into each project** — simple but duplicates ~50 lines
3. **Source-only NuGet package or shared file link** — MSBuild `<Compile Include="..\..\shared\Localizer.cs" />`

**Recommended: Option 1** — a small `ToolCommon` project keeps it DRY and testable, and the project already has multiple tool projects that could share it.

### Localization class design

```
ILocalizer
├── GetString(string key) → string  // returns English default if key missing
├── GetString(string key, params object[] args) → string  // with format args
└── CurrentLocale → string

Localizer : ILocalizer
├── Loads Localize/{locale}/text_{locale}.xml from app directory
├── Falls back to Localize/en/text_en.xml
└── Falls back to hardcoded key name if both fail
```

XML parsing reuses the same format as mods:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<localize>
    <text id="key">value</text>
</localize>
```

### Locale detection

Check in order:
1. `--locale` CLI argument (if applicable)
2. `LANG` / `LC_ALL` environment variable
3. `CultureInfo.CurrentUICulture`
4. Default to `en`

## Tasks

### Phase 1: Shared localization utility
- [ ] Create `ToolCommon/` project (net10.0) with `ILocalizer` interface and `Localizer` implementation
- [ ] XML parsing: load `<localize><text id="">` format, same as mod XML files
- [ ] File resolution: look for `Localize/{lang}/text_{lang}.xml` relative to the entry assembly location
- [ ] Fallback chain: requested locale → `en` → return key name
- [ ] Add `ToolCommon.Test/` with unit tests (mock filesystem via interface or temp files)
- [ ] Add both projects to `LobotomyCorporationMods.sln`

### Phase 2: Pilot with SetupExternal
- [ ] Create `SetupExternal/Localize/en/text_en.xml`
- [ ] Create `SetupExternal/Constants/LocalizationIds.cs`
- [ ] Replace hardcoded strings with localizer calls
- [ ] Update tests

## Key files to modify

- `SetupExternal/SetupExternal.csproj` — add ToolCommon reference, copy Localize/ to output
- `SetupExternal/Program.cs`, `FileSyncer.cs`, `Decompiler.cs` — replace strings

## Existing patterns to reuse

- XML format: identical to `LobotomyCorporationMods.GiftAlertIcon/Localize/en/text_en.xml`
- Key constants pattern: `LobotomyCorporationMods.GiftAlertIcon/Constants/LocalizationIds.cs`
- XML parsing logic (adapted from `HarmonyPatchBase.AddDefaultLocalizedText()` at `LobotomyCorporationMods.Common/Implementations/HarmonyPatchBase.cs:134-165`)

## Verification

1. `dotnet build LobotomyCorporationMods.sln` — builds cleanly
2. `dotnet test LobotomyCorporationMods.sln` — all tests pass
3. `dotnet ci --check` — formatting and analysis pass
4. Manual: run `dotnet playwright` and verify output is unchanged (English)
5. Manual: create a test `Localize/ja/text_ja.xml` with a few translated keys, run with `--locale ja`, verify translated strings appear with English fallback for untranslated keys
6. Manual: delete locale file, verify graceful fallback to English

## Risks & Considerations

- **String interpolation**: Many current strings use `$"..."` or `string.Format`. The localizer's `GetString(key, args)` must handle format arguments. Need to audit strings for complex interpolation that may not translate cleanly (e.g., different word order in other languages).
- **Multi-line formatted output**: Some tools have padded banners and tables. These may need to stay as code-constructed output rather than single localized strings.
- **Scope creep**: ~473 strings is a large migration. Consider doing it incrementally — start with command help text and user-facing messages, leave internal debug/log messages as hardcoded.
- **Testing overhead**: Each replaced string needs test updates. Using `ILocalizer` mock keeps this manageable.
