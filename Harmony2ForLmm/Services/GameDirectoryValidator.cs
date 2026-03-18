// SPDX-License-Identifier: MIT

using System.IO;
using Harmony2ForLmm.Interfaces;

namespace Harmony2ForLmm.Services
{
    /// <summary>
    /// Validates that a directory is a Lobotomy Corporation game installation.
    /// </summary>
    public sealed class GameDirectoryValidator : IGameDirectoryValidator
    {
        private const string AssemblyCSharpPath = "LobotomyCorp_Data/Managed/Assembly-CSharp.dll";

        /// <inheritdoc />
        public GameDirectoryValidationResult Validate(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return GameDirectoryValidationResult.Failure("Path is empty.");
            }

            if (!Directory.Exists(path))
            {
                return GameDirectoryValidationResult.Failure("Directory does not exist.");
            }

            var assemblyCSharp = Path.Combine(path, AssemblyCSharpPath);
            if (!File.Exists(assemblyCSharp))
            {
                return GameDirectoryValidationResult.Failure(
                    "Assembly-CSharp.dll not found. This does not appear to be a Lobotomy Corporation installation.");
            }

            return GameDirectoryValidationResult.Success();
        }
    }
}
