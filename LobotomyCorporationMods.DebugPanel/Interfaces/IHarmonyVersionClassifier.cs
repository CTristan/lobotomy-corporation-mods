// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using Hemocode.Common.Enums.Diagnostics;

#endregion

namespace Hemocode.DebugPanel.Interfaces
{
    public interface IHarmonyVersionClassifier
    {
        HarmonyVersion Classify(IList<AssemblyName> references);
    }
}
