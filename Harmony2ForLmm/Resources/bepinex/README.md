# BepInEx 5 Distribution Files

Bundled BepInEx 5 distribution for the RetargetHarmony installer.

## Pinned version

**BepInEx 5.4.23.5** (`BepInEx_win_x64_5.4.23.5.zip`)

Source: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5

## Why win_x64?

Lobotomy Corporation is a Windows-only game. Even when running on macOS/Linux
via CrossOver or Proton, it runs a Windows binary — so the Windows x64 BepInEx
distribution is always the correct one.

## Expected contents

```
bepinex/
  winhttp.dll
  doorstop_config.ini
  .doorstop_version
  BepInEx/
    core/
      (BepInEx core DLLs)
```

## Updating

When updating BepInEx, change the pinned version above, download the new
`BepInEx_win_x64` zip, and replace the files in this directory.
