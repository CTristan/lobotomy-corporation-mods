# Unit Testing Guide

Because we are unit testing mods that call Unity methods, there are certain quirks we need to work around.
These guidelines will help ensure that our tests remain consistent across environments while providing full code
coverage.

## What Methods to Test

The entry points themselves should be as simple as possible with no logic.
They should only get the necessary objects and pass them to a single extension method, all wrapped in a try/catch that
logs any exceptions.
The only tests we use for these entry point methods are that they are correctly marked with the appropriate Harmony
attributes and that any exceptions are logged.

We will only directly test the main patch method that the entry point uses, and through that any other extension methods
will be tested as they are used.

## Adapter Pattern

We use the Adapter pattern to create wrapper classes to provide an interface for calling Unity methods in unit tests
without having to write special Unity-specific try/catch statements everywhere.
The constructor takes in the Unity object we're wrapping and then the class provides a one-to-one set of methods that we
need to call.

Adapter methods should be a strict one-to-one method call to the relevant Unity method and not implement any additional
logic.
All logic should be handled by the extension method and only use the adapters to allow for unit testing.

These classes all have the ExcludeFromCodeCoverage attribute since we can't test against these classes.
At best we would just be calling the method and using a try/catch to gracefully exit; at worst we won't be able to cover
the method anyway because it accesses a Unity property and prevents us from even calling the wrapper method.

## ExcludeFromCodeCoverage Attribute

ONLY use the ExcludeFromCodeCoverage attribute for Entry Points and Adapter classes.
Any other classes should be fully testable with complete code coverage.
The attribute is provided only because of the reality that Unity does not allow us to unit test methods outside of the
Unity editor.

Entry Points are the initial Prefix/Postfix methods Harmony uses to load the patches.
These entry points must not have any logic and are just used to gather the objects needed for the actual method that
will run.
Even though we are excluding code coverage, we still need to write two tests for each entry point: that it's patched
correctly for Harmony and that it logs exceptions.

## Game Objects

Regardless of opinion on the quality of the Lobotomy Corporation codebase, the reality is that it is very difficult to
unit test.
There are no interfaces, static instances are created for almost every game system, and a lot of the classes we touch
will be tightly coupled with other unrelated classes.

To handle this situation and allow us to unit test our functionality, every game object we need to access or modify has
its own test extension for creation.
These test extensions provide the minimum amount of functionality needed to provide an instance of that object that we
can use to meaningfully test our mods.
Whenever you need to create an instance of an object, please only use the relevant test extension method.

The general pattern for these test extensions is to create an uninitialized object, forcefully populate the properties
through reflection, and return a populated instance of that object.
The reason we do this instead of just creating new instances directly is that the game was developed to initialize these
objects either when the game is loaded or during gameplay.
Because of that, many either don't have a public way of initializing them or will throw a Unity exception trying to do
so.
Starting uninitialized and populating the values manually is the only way we can reliably get any object we need.

All of the parameters for the test extension are optional, and it will automatically use a default for each parameter if
you do not provide it.
This is to keep the initialization as straight-forward as possible so that we only ask for what we need but we still get
an object that won't have null references on what we don't need.

## Test Extensions

While we have a few test extension methods that exist to reduce code duplication, the vast majority are methods to
create uninitialized Unity or Lobotomy Corporation objects.
This is because the game was developed to initialize instances of most objects either on game start or during gameplay,
and trying to create instances normally would just throw a Unity exception.

For more information, see the second on Game Objects.

## Tests run sequentially, not in parallel

You may notice in the [AssemblyInfo.cs](AssemblyInfo.cs) file that I specifically added an assembly directive to force
xUnit to run tests sequentially instead of in parallel. This is because in order to unit-test game components such as
CommandWindow I need to be able to create my own global static instances. Due to them being global and static, every
method that would use them would be affected by other tests running at the same time.

Unfortunately this is something I have no control over without modifying the game's own code directly or using my own
game DLL.

## Why is the test project .NET 6 when the mods are all .NET Framework?

Due to the nature of the Basemod-related DLLs, having the test project use .NET Framework causes various compatibility
errors. This is due to
how [.NET Framework handles references for backward-compatibility reasons](https://nickcraver.com/blog/2020/02/11/binding-redirects/),
so every update to Basemod/LMM required additional steps to get working as project references. Eventually I came across
a [compatibility error](https://stackoverflow.com/questions/403731/strong-name-validation-failed) that I wouldn't be
able to resolve without severely affecting the project and heavily modifying the workflow.

The easiest answer to this was to just use a modern .NET version so the test runner doesn't have to worry about
compatibility. Thanks to .NET Standard, modern .NET versions can load and reference .NET Framework libraries, meaning
that even though our test project isn't .NET Framework it can still run the code just fine. In other words, "it just
works".

## OK, but why specifically .NET 6 when there are more recent versions of .NET available?

As of this writing, .NET 6 is on maintenance support and will be officially out of support on November 12, 2024. The
latest LTS (Long Term Support) version is .NET 8, and .NET 9 is in Preview.

So why not upgrade? Because the way I've figured out how to unit test my Harmony patches involve creating global static
instances of the game components, such as the CommandWindow. The way I currently do that is using the .NET
FormatterServices class, which as of .NET
7 [have been marked as obsolete and are planned to no longer be available by .NET 9](https://github.com/dotnet/designs/blob/main/accepted/2020/better-obsoletion/binaryformatter-obsoletion.md).
.NET 6 is the last LTS version to still support the class and doesn't give any warnings or errors.

When trying to upgrade to .NET 8, the FormatterServices class causes an Obsolete warning, and my projects are all set to
treat warnings as errors so I actually can't compile as it's currently written. Technically I could just turn that off
and still compile, however that's just putting a bandage on the issue and doesn't solve the fact that it will eventually
be removed.

Having said all of that, I am researching ways to do what I am currently doing without using the FormatterServices
class, but right now it's not likely to be a priority for me. After all, this is to run unit tests against .NET
Framework 4 mods that will be used with a game that was created in .NET Framework 3.5. Being a few versions behind is
honestly the least of our concerns.
