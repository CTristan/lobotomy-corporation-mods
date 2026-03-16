// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Reflection;
using LobotomyCorporationMods.Common.Enums.Diagnostics;

#endregion

namespace LobotomyCorporationMods.DebugPanel.Interfaces
{
    public interface IHarmonyVersionClassifier
    {
        HarmonyVersion Classify(IList<AssemblyName> references);
    }
}
