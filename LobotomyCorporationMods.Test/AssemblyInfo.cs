// SPDX-License-Identifier: MIT

using Xunit;

// Causes all unit tests to run sequentially instead of in parallel.
// Most of our tests touch global static instance objects, so this prevents them from affecting each other.
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace LobotomyCorporationMods.Test
{
    public class AssemblyInfo
    {
    }
}
