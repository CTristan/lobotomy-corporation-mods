// SPDX-License-Identifier: MIT

#region

using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    internal static class PeTestHelper
    {
        public static byte[] BuildAssemblyWithRefs(params string[] refNames)
        {
            var metadata = new MetadataBuilder();

            metadata.AddModule(
                0,
                metadata.GetOrAddString("test.dll"),
                metadata.GetOrAddGuid(Guid.NewGuid()),
                default,
                default);

            metadata.AddAssembly(
                metadata.GetOrAddString("TestAssembly"),
                new Version(1, 0, 0, 0),
                default,
                default,
                default,
                AssemblyHashAlgorithm.None);

            metadata.AddTypeDefinition(
                default,
                default,
                metadata.GetOrAddString("<Module>"),
                default,
                MetadataTokens.FieldDefinitionHandle(1),
                MetadataTokens.MethodDefinitionHandle(1));

            foreach (var name in refNames)
            {
                metadata.AddAssemblyReference(
                    metadata.GetOrAddString(name),
                    new Version(1, 0, 0, 0),
                    default,
                    default,
                    default,
                    default);
            }

            var rootBuilder = new MetadataRootBuilder(metadata);
            var peHeaderBuilder = new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll);
            var ilStream = new BlobBuilder();

            var peBuilder = new ManagedPEBuilder(
                peHeaderBuilder,
                rootBuilder,
                ilStream);

            var output = new BlobBuilder();
            peBuilder.Serialize(output);

            return output.ToArray();
        }
    }
}
