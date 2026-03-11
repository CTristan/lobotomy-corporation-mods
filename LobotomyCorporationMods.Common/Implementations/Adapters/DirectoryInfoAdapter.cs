// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using System.IO;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class DirectoryInfoAdapter : IDirectoryInfo
    {
        private readonly DirectoryInfo _directoryInfo;

        public DirectoryInfoAdapter([NotNull] DirectoryInfo directoryInfo)
        {
            ThrowHelper.ThrowIfNull(directoryInfo, nameof(directoryInfo));
            _directoryInfo = directoryInfo;
        }

        public string FullName => _directoryInfo.FullName;
    }
}
