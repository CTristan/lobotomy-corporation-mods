// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using UnityEngine;
using WorkerSprite;

namespace LobotomyCorporationMods.Test
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    internal static class TestData
    {
        internal const long DefaultAgentId = 1L;

        internal const string DefaultAgentName = "DefaultAgentName";

        internal const CustomizingType DefaultCustomizingType = CustomizingType.GENERATE;

        internal const string DefaultSpriteName = "DefaultSpriteName";

        [NotNull] internal static AgentData DefaultAgentData => TestExtensions.CreateAgentData(DefaultAppearance);

        [NotNull] internal static AgentModel DefaultAgentModel => TestExtensions.CreateAgentModel(DefaultAgentId, DefaultAgentName, DefaultWorkerSprite);

        [NotNull] internal static Appearance DefaultAppearance => TestExtensions.CreateAppearance(DefaultWorkerSprite);

        [NotNull] internal static AppearanceUI DefaultAppearanceUI => TestExtensions.CreateAppearanceUI();

        [NotNull]
        internal static CustomizingWindow DefaultCustomizingWindow => TestExtensions.CreateCustomizingWindow(DefaultAppearanceUI, DefaultAgentModel, DefaultAgentData, DefaultCustomizingType);

        [NotNull] internal static Sprite DefaultSprite => TestExtensions.CreateSprite(DefaultSpriteName);

        [NotNull] internal static WorkerBasicSpriteController DefaultWorkerBasicSpriteController => TestExtensions.CreateWorkerBasicSpriteController();

        [NotNull] internal static WorkerSprite.WorkerSprite DefaultWorkerSprite => TestExtensions.CreateWorkerSprite();
    }
}
