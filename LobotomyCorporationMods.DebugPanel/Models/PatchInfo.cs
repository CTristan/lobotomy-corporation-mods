// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Models
{
    public sealed class PatchInfo
    {
        public PatchInfo(string targetType, string targetMethod, PatchType patchType, string owner, string patchMethod, string patchAssemblyName)
        {
            TargetType = Guard.Against.Null(targetType, nameof(targetType));
            TargetMethod = Guard.Against.Null(targetMethod, nameof(targetMethod));
            PatchType = patchType;
            Owner = Guard.Against.Null(owner, nameof(owner));
            PatchMethod = Guard.Against.Null(patchMethod, nameof(patchMethod));
            PatchAssemblyName = patchAssemblyName ?? string.Empty;
        }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public PatchType PatchType { get; private set; }

        public string Owner { get; private set; }

        public string PatchMethod { get; private set; }

        public string PatchAssemblyName { get; private set; }
    }
}
