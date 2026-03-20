// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;
using UnityEngine;

#endregion

namespace DebugPanel.Implementations
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
