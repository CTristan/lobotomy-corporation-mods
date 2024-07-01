// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IWorkerSpriteManagerTestAdapter : ITestAdapter<WorkerSpriteManager>
    {
        void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear);
    }
}
