// SPDX-License-Identifier:MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [ExcludeFromCodeCoverage]
    public sealed class WorkerSpriteManagerAdapter : IWorkerSpriteManagerAdapter
    {
        private readonly WorkerSpriteManager _manager;

        public WorkerSpriteManagerAdapter(WorkerSpriteManager manager)
        {
            _manager = manager;
        }

        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite, Appearance appear)
        {
            _manager.SetAgentBasicData(workerSprite, appear);
        }
    }
}
