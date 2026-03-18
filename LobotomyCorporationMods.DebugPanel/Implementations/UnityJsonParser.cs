// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.DebugPanel.Interfaces;
using UnityEngine;

#endregion

namespace Hemocode.DebugPanel.Implementations
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
