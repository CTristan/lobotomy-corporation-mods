// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IWorkerSpriteManagerTestAdapter : IComponentTestAdapter<WorkerSpriteManager>
    {
        void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite, Appearance appear);
    }
}
