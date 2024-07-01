// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class WorkerSpriteManagerTestAdapter : Adapter<WorkerSpriteManager>, IWorkerSpriteManagerTestAdapter
    {
        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear)
        {
            GameObject.SetAgentBasicData(workerSprite, appear);
        }
    }
}
