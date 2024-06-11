# Contributing

## Development

### Environment setup

The original game files are required but are not provided.
My current environment setup is a "`src`" folder in the BaseMods folder that I placed the repo in, so my folder
structure for the repo is LobotomyCorp_Data/BaseMods/src/lobotomy-corporation-mods.
If you follow this same structure then the references should use the game’s files and will build the output to the
appropriate BaseMod folder e.g. LobotomyCorp_Data/BaseMods/LobotomyCorporationMods.BadLuckProtectionForGifts.

If you’re running Linux, make sure mono-devel is installed.
You should be able to open the solution in VSCode or VSCodium and build with xbuild.
That said, I wouldn't recommend it as I have not found a way to be able to debug Unity .NET Framework DLLs in Linux like
I can in Windows with dnSpy, especially since the game has to run in Proton.
If you are able to get debugging working in Linux I would love to hear about it!

### Coding standards

Due to the nature of Harmony patching, there are going to be some quirks on how to do things that you wouldn't do in a
sane codebase.

- Mod projects need to use .NET Framework 3.5.
    - The original game was created an old Unity version that used .NET Framework 3.5, so we need to use the same
      version for our mods.
- Every mod needs to have a "Harmony_Patch" class in the project root.
    - Basemod requires this name for the class that initializes the Harmony patches that the mod will load.
- Harmony patches must be Postfix unless the patch will not work if it's not Prefix. If Prefix is required, a comment
  needs to display why Postfix won't work.
    - Prefix methods are potentially dangerous for both the game and other mods since you're changing game state before
      the method runs. Running our Postfix patch allows us to do things after the method ran so the game and other mods
      are much less likely to misbehave.
    - That said, the patches only need to be Postfix when shared with others, so I would recommend creating your patches
      using Prefix for best-case scenario, then when everything works as expected changing all of your patches to
      Postfix and re-testing your mod to make sure they don't break.
- Never use "== null" or "!= null", always use "is null" or "is object" respectively.
    - Because of how [Unity overwrites the == operator for null checking](https://stackoverflow.com/a/72072517), it
      breaks unit tests because we're not actually running Unity to run the tests. When we use "is null" then it doesn't
      use the overloaded operator and checks if it's actually null.
    - While this isn't true for our own types, it's better to be safe and just mandate it everywhere so we don't have to
      double-check whether we're checking a Unity type or not.
