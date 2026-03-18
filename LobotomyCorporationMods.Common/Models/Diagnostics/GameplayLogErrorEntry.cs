// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Implementations;

#endregion

namespace Hemocode.Common.Models.Diagnostics
{
    public sealed class GameplayLogErrorEntry
    {
        public GameplayLogErrorEntry(string modName, string dllName, string errorMessage, string stackTrace, string rawLine)
        {
            ThrowHelper.ThrowIfNull(modName);
            ModName = modName;
            ThrowHelper.ThrowIfNull(dllName);
            DllName = dllName;
            ThrowHelper.ThrowIfNull(errorMessage);
            ErrorMessage = errorMessage;
            ThrowHelper.ThrowIfNull(stackTrace);
            StackTrace = stackTrace;
            ThrowHelper.ThrowIfNull(rawLine);
            RawLine = rawLine;
        }

        public string ModName { get; private set; }

        public string DllName { get; private set; }

        public string ErrorMessage { get; private set; }

        public string StackTrace { get; private set; }

        public string RawLine { get; private set; }
    }
}
