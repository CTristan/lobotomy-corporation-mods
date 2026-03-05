// SPDX-License-Identifier: MIT

using System;

namespace HarmonyDebugPanel.Models
{
    public sealed class PatchInfo
    {
        public PatchInfo()
            : this(string.Empty, string.Empty, PatchType.Prefix, string.Empty, string.Empty)
        {
        }

        public PatchInfo(string targetType, string targetMethod, PatchType patchType, string owner, string patchMethod)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            PatchType = patchType;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            PatchMethod = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));
        }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public PatchType PatchType { get; private set; }

        public string Owner { get; private set; }

        public string PatchMethod { get; private set; }
    }
}
