// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class ExpectedPatchInfo
    {
        public ExpectedPatchInfo(string patchAssembly, string targetType, string targetMethod, string patchMethod, PatchType patchType)
        {
            PatchAssembly = Guard.Against.Null(patchAssembly, nameof(patchAssembly));
            TargetType = Guard.Against.Null(targetType, nameof(targetType));
            TargetMethod = Guard.Against.Null(targetMethod, nameof(targetMethod));
            PatchMethod = Guard.Against.Null(patchMethod, nameof(patchMethod));
            PatchType = patchType;
        }

        public string PatchAssembly { get; private set; }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public string PatchMethod { get; private set; }

        public PatchType PatchType { get; private set; }
    }
}
