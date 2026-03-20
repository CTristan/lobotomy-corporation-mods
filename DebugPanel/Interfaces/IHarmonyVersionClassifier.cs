// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using DebugPanel.Common.Enums.Diagnostics;

#endregion

namespace DebugPanel.Interfaces
{
    public interface IHarmonyVersionClassifier
    {
        HarmonyVersion Classify(IList<AssemblyName> references);
    }
}
