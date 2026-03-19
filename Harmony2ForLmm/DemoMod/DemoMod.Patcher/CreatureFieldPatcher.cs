// SPDX-License-Identifier: MIT

#region Using directives

using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Mono.Cecil;

#endregion

namespace DemoMod.Patcher
{
    /// <summary>
    /// Demonstrates: Preloader patchers (§Preloader patchers).
    /// Adds a public int field to CreatureModel before the game loads the assembly.
    /// </summary>
    public static class CreatureFieldPatcher
    {
        public static IEnumerable<string> TargetDLLs { get; } = new[] { "Assembly-CSharp.dll" };

        public static void Patch(AssemblyDefinition assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            using (var logger = Logger.CreateLogSource("CreatureFieldPatcher"))
            {
                // Find CreatureModel in Assembly-CSharp
                TypeDefinition creatureModel = null;
                foreach (var type in assembly.MainModule.Types)
                {
                    if (type.Name == "CreatureModel")
                    {
                        creatureModel = type;

                        break;
                    }
                }

                if (creatureModel == null)
                {
                    logger.LogError("CreatureModel type not found");

                    return;
                }

                // Add a public int field for custom difficulty
                var intRef = assembly.MainModule.ImportReference(typeof(int));
                var field = new FieldDefinition(
                    "customDifficultyLevel",
                    FieldAttributes.Public,
                    intRef);
                creatureModel.Fields.Add(field);

                logger.LogInfo("Added customDifficultyLevel field to CreatureModel");
            }
        }
    }
}
