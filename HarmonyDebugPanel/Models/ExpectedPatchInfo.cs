// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class ExpectedPatchInfo(string patchAssembly, string targetType, string targetMethod, string patchMethod, PatchType patchType)
    {
        public string PatchAssembly { get; private set; } = patchAssembly ?? throw new ArgumentNullException(nameof(patchAssembly));

        public string TargetType { get; private set; } = targetType ?? throw new ArgumentNullException(nameof(targetType));

        public string TargetMethod { get; private set; } = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));

        public string PatchMethod { get; private set; } = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));

        public PatchType PatchType { get; private set; } = patchType;
    }
}
