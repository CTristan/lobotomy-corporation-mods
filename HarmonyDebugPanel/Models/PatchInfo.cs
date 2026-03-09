// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class PatchInfo(string targetType, string targetMethod, PatchType patchType, string owner, string patchMethod, string patchAssemblyName)
    {
        public PatchInfo()
            : this(string.Empty, string.Empty, PatchType.Prefix, string.Empty, string.Empty, string.Empty)
        {
        }

        public string TargetType { get; private set; } = targetType ?? throw new ArgumentNullException(nameof(targetType));

        public string TargetMethod { get; private set; } = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));

        public PatchType PatchType { get; private set; } = patchType;

        public string Owner { get; private set; } = owner ?? throw new ArgumentNullException(nameof(owner));

        public string PatchMethod { get; private set; } = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));

        public string PatchAssemblyName { get; private set; } = patchAssemblyName ?? string.Empty;
    }
}
