// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.Common.Models.Diagnostics
{
    public sealed class ExpectedPatchInfo
    {
        public ExpectedPatchInfo(string patchAssembly, string targetType, string targetMethod, string patchMethod, PatchType patchType)
        {
            ThrowHelper.ThrowIfNull(patchAssembly);
            PatchAssembly = patchAssembly;
            ThrowHelper.ThrowIfNull(targetType);
            TargetType = targetType;
            ThrowHelper.ThrowIfNull(targetMethod);
            TargetMethod = targetMethod;
            ThrowHelper.ThrowIfNull(patchMethod);
            PatchMethod = patchMethod;
            PatchType = patchType;
        }

        public string PatchAssembly { get; private set; }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public string PatchMethod { get; private set; }

        public PatchType PatchType { get; private set; }
    }
}
