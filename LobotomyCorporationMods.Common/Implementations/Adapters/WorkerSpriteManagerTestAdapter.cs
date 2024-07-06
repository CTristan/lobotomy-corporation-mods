// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Attributes;
using LobotomyCorporationMods.Common.Constants;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.Common.Implementations.Adapters
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class WorkerSpriteManagerTestAdapter : ComponentTestAdapter<WorkerSpriteManager>, IWorkerSpriteManagerTestAdapter
    {
        internal WorkerSpriteManagerTestAdapter([NotNull] WorkerSpriteManager gameObject) : base(gameObject)
        {
        }

        public void SetAgentBasicData(WorkerSprite.WorkerSprite workerSprite,
            Appearance appear)
        {
            GameObject.SetAgentBasicData(workerSprite, appear);
        }
    }
}
