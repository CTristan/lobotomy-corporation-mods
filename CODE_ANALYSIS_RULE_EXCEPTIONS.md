# Code Analysis Rule Exceptions

## Overview

Static code analysis is extremely helpful for finding code issues, and I try to
keep as many rules enabled as possible. However, due to the nature of modding,
we can't control how we interface with the game code. Some rules just will not
work for us and have to be disabled. Because I tend to re-enable rules because I
forgot why I disabled them, I wanted to document the reasons why they're
disabled.

## SonarLint

### Changed Rules

* S2436: Types and methods shouldn’t have too many generic parameters
  * I've changed this rule to allow up to 3 generic parameters instead of the
    default 2. This is because sometimes I need to use three generic parameters
    for handling adapter classes for unit testing.

### Disabled Rules

* S101: Types should be named in PascalCase
  * This triggers off of the "Harmony_Patch" class names for each mod.
    Unfortunately, this is a hardcoded class name in Basemod, so we have no say
    here.
* S2339: Public constant members shouldn't be used
  * Conflicts with Code Analysis
    rule [CA2211: Non-constant fields should not be visible](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca2211).
* S2360: Optional parameters shouldn't be used
  * To enable unit testing, I've had to create interfaces for wrapper classes
    for actual game objects. As such, these test adapters are being passed
    around throughout the codebase. To simplify the actual game method calls to
    not have to care about the adapters, I've defaulted them all to null and set
    them to use the actual game objects when null. If I left this rule enabled,
    I would either need to duplicate a lot of methods or have adapter setup code
    in every Harmony Patch method; both options would be unmaintainable over the
    long run.
* S3011: Reflection shouldn't be used to increase accessibility of classes,
  methods, or fields
  * We need to be able to see and modify private properties and methods since
    the game code doesn't have a public way to interact with them.
* S3242: Method parameters should be declared with base types
  * There are two reasons this won't work for us:
    1. Patch methods might require a parameter that the game passes in or out,
       and that parameter might have a base type that could be used instead. If
       we use the base type instead inside our patch method, then it may not
       pass through correctly or have side effects when used later on.
    2. This conflicts with the Code Analysis
       rule [CA1859: Use concrete types when possible for improved performance](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1859)
       when the base type is abstract, and we can't change the base type if it's
       game code.
* S3874: "out" and "ref" parameters should not be used
  * Harmony patching requires "ref" parameters to modify game state.
* S6354: Use a testable date/time provider
  * I actually don't like this rule for one very simple reason:
    [There actually is no way to have this enabled without having to suppress the warning at least once.](https://github.com/SonarSource/sonar-dotnet/issues/5927)
    If I'm going to have to suppress it anyway, I might as well just not worry
    about it in the first place.
