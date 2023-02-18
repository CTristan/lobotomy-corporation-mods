// SPDX-License-Identifier:MIT

#region

using Customizing;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces.Adapters
{
    public interface IWorkerSpriteManagerAdapter
    {
        void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite, Appearance appear);
    }
}
