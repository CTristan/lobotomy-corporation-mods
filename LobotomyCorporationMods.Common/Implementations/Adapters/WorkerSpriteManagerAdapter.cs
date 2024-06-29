// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class WorkerSpriteManagerAdapter : Adapter<WorkerSpriteManager>, IWorkerSpriteManagerAdapter
    {
        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear)
        {
            GameObject.SetAgentBasicData(workerSprite, appear);
        }
    }
}
