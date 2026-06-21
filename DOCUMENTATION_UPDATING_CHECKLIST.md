# Releasing a Mod

Each mod is released on its own, without touching any other mod's release. The
mod's `.csproj` `<AssemblyVersion>` is the single source of truth for its
version; everything else is synced or generated from it.

## 1. Bump the version and update docs (in a pull request)

- [ ] `<Mod>/<Mod>.csproj` — set `<AssemblyVersion>` to the new version.
- [ ] Sync the in-game display names: run
      `scripts/check-versions.sh --write <ModId>`. This rewrites the `v<X.Y.Z>`
      suffix in every `Info/{lang}/Info.xml` `<name>` from the csproj version.
- [ ] `<Mod>/README.md` — add a changelog entry.
- [ ] `CHANGELOG.md` (root) — add a player-facing entry. Only add changes a
      player would notice (features, gameplay fixes, UI, new options); not infra,
      tooling, tests, refactors, or docs.
- [ ] `INTEGRATION_TESTING_CHECKLIST.md` — update if the test steps changed.
- [ ] Open the pull request. CI runs the version drift gate
      (`scripts/check-versions.sh --check all`) alongside the build and tests.

## 2. Release (after the pull request merges to `main`)

- [ ] Tag the merge commit and push it:

      git tag <ModId>-v<X.Y.Z>          # e.g. BugFixes-v3.0.2
      git push origin <ModId>-v<X.Y.Z>

- [ ] The **Release mod** workflow rebuilds the whole mod set and publishes a
      dated snapshot release (`Mods — <date>`) marked **Latest**, containing
      every mod's zip plus `all-mods.zip`.

## 3. Nexus

- [ ] Upload the changed mod to Nexus by hand — see
      [releasing/NEXUS_UPLOAD.md](releasing/NEXUS_UPLOAD.md). (Automated upload is
      planned as Phase 3b of the release pipeline.)

## Notes

- The version is cosmetic to LMM/Basemod (they key off the mod folder name and
  `Info/GlobalInfo.xml <ID>`), but it is shown to users, so keep it in sync.
- Most mods use normal SemVer. `WarnWhenAgentWillDieFromWorking` is the
  exception: its **major version is the number of abnormalities it warns about**
  (see that mod's README).
