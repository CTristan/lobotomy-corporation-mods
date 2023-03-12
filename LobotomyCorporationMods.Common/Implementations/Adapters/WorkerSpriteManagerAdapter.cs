// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Customizing;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage]
    public sealed class WorkerSpriteManagerAdapter : ComponentAdapter, IWorkerSpriteManagerAdapter
    {
        private WorkerSpriteManager? _workerSpriteManager;

        public new WorkerSpriteManager GameObject
        {
            get
            {
                if (_workerSpriteManager is null)
                {
                    throw new InvalidOperationException(UninitializedGameObjectErrorMessage);
                }

                return _workerSpriteManager;
            }
            set => _workerSpriteManager = value;
        }

        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite, Appearance appear)
        {
            GameObject.SetAgentBasicData(workerSprite, appear);
        }
    }
}
