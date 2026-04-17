<!--
Transient plan document. Delete when this repo's rollout is complete.
-->

# Testing standards sync — lobotomy-corporation-mods

Scoped slice of the org-wide initiative. Master plan: https://github.com/open-lobotomy/.github/blob/main/.github/plans/testing-standards-sync.md

## Context

One test project (covers all mods), one rename, and two CLAUDE.md corrections. The "Two-Method Patch Pattern" framing stays — it's the Harmony-specific expression of the Category 1/2 pattern in canonical TESTING.md.

## This repo's work

### Test-project rename
- [ ] `LobotomyCorporationMods.Test/` → `LobotomyCorporationMods.Tests/` (folder, `.csproj`, `LobotomyCorporationMods.slnx` entry, namespace blocks in test files, `Directory.Build.props` or any global file that names the test project, any `<InternalsVisibleTo>` targets).

### IVT audit (three-way classification)
- [ ] Each mod's `Harmony_Patch` entry-point `.csproj` is an **application** (Unity mod, Harmony-patched DLL entry). IVT is allowed under the new rule — the blanket "Never use `InternalsVisibleTo`" guidance in the current CLAUDE.md is superseded.
- [ ] `LobotomyCorporationMods.Common` (the local development directory) is a **library** when treated as a standalone package; matches the NuGet-packaged `LobotomyCorporation.Mods.Common` in `open-lobotomy-tooling`. No IVT.
- [ ] Decide: keep the existing `public`-everywhere conventions if they're still warranted, or opt into IVT on a per-mod basis where it simplifies testing. Not urgent; the rule change permits but doesn't require it.

### TESTING.md drop-in
- [ ] Merge the incoming `automated-sync` PR that adds `TESTING.md` at repo root.

### CLAUDE.md updates
- [ ] Reduce the `## Coding Conventions` and `### Test Project` sections of `.claude/CLAUDE.md`:
  - [ ] Remove the rule "Never use `InternalsVisibleTo` — make code public or test indirectly through public methods." Superseded by the three-way classification in TESTING.md (the mods here are applications, IVT is allowed).
  - [ ] Correct "100% coverage target for business logic" to match the actual `coverlet.json` threshold of 80/80/80. (The actual file says 80; the CLAUDE.md was aspirational and drifted from ground truth.)
- [ ] Preserve the "Two-Method Patch Pattern" section, but cross-reference it to TESTING.md's Category 1/2 framing: explicitly note that the extension method is Category 1 (fully testable) and the `[HarmonyPostfix]` method with `[ExcludeFromCodeCoverage]` is Category 2.
- [ ] Preserve the "Documentation Checklist" and other repo-specific conventions as-is.

### Tooling pin bump
- [ ] After `open-lobotomy-tooling` ships the `check-testing-doc` subcommand, bump the pin in this repo so CI picks up the failsafe.

## Verification

- [ ] `dotnet build LobotomyCorporationMods.slnx` passes.
- [ ] `dotnet test LobotomyCorporationMods.slnx` — all mod tests pass; coverage report lists the renamed test project.
- [ ] `dotnet ci --check` passes.
- [ ] Patch tests (`HarmonyPatchTests.cs` in each `ModTests/{ModName}Tests/` directory) still execute; `[EntryPoint]` and `[ExcludeFromCodeCoverage]` attribute handling unchanged.
- [ ] Opening this repo on github.com shows `TESTING.md` at root with correct version header.

## Notes

- **Largest rename surface** of the 5 repos. The single `LobotomyCorporationMods.Test` project covers every mod, so its project name and namespace root appear in many test files. Use `git grep` before and after to confirm nothing was missed.
- **Two-Method Patch Pattern stays.** It's the concrete Harmony expression of Category 1/2. Moving it into canonical TESTING.md would over-specialize the org-wide doc to Harmony; keeping it here keeps TESTING.md framework-agnostic while preserving the pattern documentation for mod authors.
- **Submodule coordination:** `open-lobotomy-tooling` is a submodule here. The tooling repo's rename PR lands first; bump the submodule pin as part of this repo's rollout PR.
