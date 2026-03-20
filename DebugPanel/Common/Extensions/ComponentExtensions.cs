// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using UnityEngine;

namespace DebugPanel.Common.Extensions
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public static class ComponentExtensions
    {
        internal static bool IsUnityNull(this Component component)
        {
            return !component;
        }
    }
}
