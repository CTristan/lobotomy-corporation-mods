// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyDebugPanel.Interfaces;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Interfaces
{
    public sealed class PatchInspectionInfo
    {
        /// <summary>
        /// Initializes a new instance of PatchInspectionInfo.
        /// </summary>
        /// <param name="targetType">The target type being patched.</param>
        /// <param name="targetMethod">The target method being patched.</param>
        /// <param name="patchType">The type of patch (Prefix, Postfix, Transpiler, etc.).</param>
        /// <param name="owner">The owner of the patch.</param>
        /// <param name="patchMethod">The patch method name.</param>
        /// <param name="patchAssemblyName">The patch assembly name. Use empty string for "no assembly name". Null is not allowed.</param>
        /// <param name="patchAssemblyVersion">The patch assembly version.</param>
        /// <param name="patchAssemblyReferences">The patch assembly references.</param>
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
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            PatchType = patchType;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            PatchMethod = patchMethod ?? throw new ArgumentNullException(nameof(patchMethod));
            PatchAssemblyName = patchAssemblyName ?? throw new ArgumentNullException(nameof(patchAssemblyName));
            PatchAssemblyVersion = patchAssemblyVersion ?? throw new ArgumentNullException(nameof(patchAssemblyVersion));
            PatchAssemblyReferences = patchAssemblyReferences ?? throw new ArgumentNullException(nameof(patchAssemblyReferences));
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
