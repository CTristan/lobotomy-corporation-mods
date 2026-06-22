# Nexus upload checklist (manual)

Until the automated Nexus upload lands (release pipeline Phase 3b — tracked in #172), upload the
changed mod to Nexus Mods by hand after the GitHub snapshot release is published.

This is per-mod: upload only the mod whose version you just changed. The other
mods on Nexus are not affected.

## Mod to Nexus page

| Mod (ModId) | Nexus page |
| --- | --- |
| BadLuckProtectionForGifts | <https://www.nexusmods.com/lobotomycorporation/mods/476> |
| BugFixes | <https://www.nexusmods.com/lobotomycorporation/mods/478> |
| FreeCustomization | <https://www.nexusmods.com/lobotomycorporation/mods/477> |
| GiftAlertIcon | <https://www.nexusmods.com/lobotomycorporation/mods/494> |
| NotifyWhenAgentReceivesGift | <https://www.nexusmods.com/lobotomycorporation/mods/487> |
| WarnWhenAgentWillDieFromWorking | <https://www.nexusmods.com/lobotomycorporation/mods/479> |

DontChatMe is not on Nexus yet. Its page must be created once by hand before its
first upload (the Nexus upload API cannot create new pages).

## Steps

1. Confirm the GitHub snapshot release was published and is marked **Latest**.
2. Download the changed mod's `<ModId>.zip` from that release's assets.
3. Open the mod's Nexus page (table above) and sign in as the mod author.
4. Add a new file:
   - Upload `<ModId>.zip`.
   - Set the version to match the mod's new version — the csproj
     `<AssemblyVersion>` (for example, `3.0.2`).
   - Write a short description of what changed (mirror the mod's changelog).
5. Publish, then check that the new version shows on the page.
