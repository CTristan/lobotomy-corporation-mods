// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using DebugPanel.Common.Implementations;
using DebugPanel.Common.Enums.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public sealed class PatchInspectionInfo
    {
        public PatchInspectionInfo(
            string targetType,
            string targetMethod,
            PatchType patchType,
            string owner,
            string patchMethod,
            string patchAssemblyName,
            string patchAssemblyVersion,
            IList<AssemblyName> patchAssemblyReferences)
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
            ThrowHelper.ThrowIfNull(patchAssemblyName);
            PatchAssemblyName = patchAssemblyName;
            ThrowHelper.ThrowIfNull(patchAssemblyVersion);
            PatchAssemblyVersion = patchAssemblyVersion;
            ThrowHelper.ThrowIfNull(patchAssemblyReferences);
            PatchAssemblyReferences = patchAssemblyReferences;
        }

        public string TargetType { get; private set; }

        public string TargetMethod { get; private set; }

        public PatchType PatchType { get; private set; }

        public string Owner { get; private set; }

        public string PatchMethod { get; private set; }

        public string PatchAssemblyName { get; private set; }

        public string PatchAssemblyVersion { get; private set; }

        public IList<AssemblyName> PatchAssemblyReferences { get; private set; }
    }
}
