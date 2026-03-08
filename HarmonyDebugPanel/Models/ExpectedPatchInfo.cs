// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class ExpectedPatchInfo
    {
        public ExpectedPatchInfo(string patchAssembly, string targetType, string targetMethod, string patchMethod, PatchType patchType)
        {
            PatchAssembly = patchAssembly ?? throw new ArgumentNullException(nameof(patchAssembly));
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            PatchMethod = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));
            PatchType = patchType;
        }

        public string PatchAssembly { get; private set; }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public string PatchMethod { get; private set; }

        public PatchType PatchType { get; private set; }
    }
}
