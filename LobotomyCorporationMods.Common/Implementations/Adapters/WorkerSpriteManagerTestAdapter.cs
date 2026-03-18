// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using Hemocode.Common.Attributes;
using Hemocode.Common.Constants;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;

#endregion

namespace Hemocode.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class WorkerSpriteManagerTestAdapter : ComponentTestAdapter<WorkerSpriteManager>, IWorkerSpriteManagerTestAdapter
    {
        internal WorkerSpriteManagerTestAdapter([NotNull] WorkerSpriteManager gameObject) : base(gameObject)
        {
        }

        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear)
        {
            GameObjectInternal.SetAgentBasicData(workerSprite, appear);
        }
    }
}
