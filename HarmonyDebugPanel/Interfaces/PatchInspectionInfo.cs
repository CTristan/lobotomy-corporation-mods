// SPDX-License-Identifier: MIT

using System;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class PatchInspectionInfo
    {
        public PatchInspectionInfo(string targetType, string targetMethod, PatchType patchType, string owner, string patchMethod)
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
