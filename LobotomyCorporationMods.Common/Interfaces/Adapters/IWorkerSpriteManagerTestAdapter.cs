// SPDX-License-Identifier: MIT

#region

using Customizing;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;

#endregion

namespace Hemocode.Common.Interfaces.Adapters
{
    public interface IWorkerSpriteManagerTestAdapter : IComponentTestAdapter<WorkerSpriteManager>
    {
        void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear);
    }
}
