// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.DebugPanel.Models;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
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
            TargetType = Guard.Against.Null(targetType, nameof(targetType));
            TargetMethod = Guard.Against.Null(targetMethod, nameof(targetMethod));
            PatchType = patchType;
            Owner = Guard.Against.Null(owner, nameof(owner));
            PatchMethod = Guard.Against.Null(patchMethod, nameof(patchMethod));
            PatchAssemblyName = Guard.Against.Null(patchAssemblyName, nameof(patchAssemblyName));
            PatchAssemblyVersion = Guard.Against.Null(patchAssemblyVersion, nameof(patchAssemblyVersion));
            PatchAssemblyReferences = Guard.Against.Null(patchAssemblyReferences, nameof(patchAssemblyReferences));
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
