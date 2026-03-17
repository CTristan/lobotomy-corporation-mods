// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.DebugPanel.Interfaces;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class UnityJsonParser : IJsonParser
    {
        public T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}
