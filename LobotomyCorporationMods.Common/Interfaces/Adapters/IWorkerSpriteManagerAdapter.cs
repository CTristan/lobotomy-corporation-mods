// SPDX-License-Identifier: MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IWorkerSpriteManagerAdapter : IAdapter<WorkerSpriteManager>, IComponentAdapter
    {
        new WorkerSpriteManager GameObject { get; set; }
        new IGameObjectAdapter GameObjectAdapter { get; }
        void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite, Appearance appear);
    }
}
