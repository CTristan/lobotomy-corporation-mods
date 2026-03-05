// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Reflection;
using HarmonyDebugPanel.Models;

namespace HarmonyDebugPanel.Interfaces
{
    public interface IHarmonyVersionClassifier
    {
        HarmonyVersion Classify(IList<AssemblyName> references);
    }
}
