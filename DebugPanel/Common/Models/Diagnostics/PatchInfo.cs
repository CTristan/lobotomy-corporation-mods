// SPDX-License-Identifier: MIT

#region

using DebugPanel.Common.Enums.Diagnostics;
using DebugPanel.Common.Implementations;

#endregion

namespace DebugPanel.Common.Models.Diagnostics
{
    public sealed class PatchInfo
    {
        public PatchInfo(string targetType, string targetMethod, PatchType patchType, string owner, string patchMethod, string patchAssemblyName)
        {
            ThrowHelper.ThrowIfNull(targetType);
            TargetType = targetType;
            ThrowHelper.ThrowIfNull(targetMethod);
            TargetMethod = targetMethod;
            PatchType = patchType;
            ThrowHelper.ThrowIfNull(owner);
            Owner = owner;
            ThrowHelper.ThrowIfNull(patchMethod);
            PatchMethod = patchMethod;
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
