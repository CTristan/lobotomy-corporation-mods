// SPDX-License-Identifier: MIT

#region

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterContainers;
using UnityEngine;

#endregion

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ManagementSlotTestAdapterExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this IManagementSlotTestAdapter managementSlotTestAdapter,
            [NotNull] ImageParametersContainer imageParametersContainer,
            [CanBeNull] OptionalTestAdapterParametersContainer testAdapterParametersContainer = null)
        {
            testAdapterParametersContainer = testAdapterParametersContainer.EnsureNotNullWithMethod(() => new OptionalTestAdapterParametersContainer());
            testAdapterParametersContainer.SpriteTestAdapter = testAdapterParametersContainer.SpriteTestAdapter.EnsureNotNullWithMethod(() => new SpriteTestAdapter(new Sprite()));
            testAdapterParametersContainer.Texture2DTestAdapter = testAdapterParametersContainer.Texture2DTestAdapter.EnsureNotNullWithMethod(() => new Texture2dTestAdapter());
            testAdapterParametersContainer.GameObjectTestAdapter = testAdapterParametersContainer.GameObjectTestAdapter.EnsureNotNullWithMethod(() => new GameObjectTestAdapter(new GameObject()));

            var imageGameObjectTestAdapter = testAdapterParametersContainer.GameObjectTestAdapter;
            var parent = managementSlotTestAdapter.Transform.GetChild(0);
            imageGameObjectTestAdapter.Transform.SetParent(parent);
            imageGameObjectTestAdapter.Transform.LocalScale = new Vector3(imageParametersContainer.LocalScaleX, imageParametersContainer.LocalScaleY);
            imageGameObjectTestAdapter.Transform.LocalPosition = new Vector3(imageParametersContainer.LocalPositionX, imageParametersContainer.LocalPositionY, imageParametersContainer.LocalPositionZ);
            imageGameObjectTestAdapter.SetActive(true);

            var texture2dTestAdapter = testAdapterParametersContainer.Texture2DTestAdapter;
            var sprite = testAdapterParametersContainer.SpriteTestAdapter.Create(texture2dTestAdapter.GameObject, new Rect(0f, 0f, texture2dTestAdapter.Width, texture2dTestAdapter.Height),
                new Vector2(0.5f, 0.5f));

            var imageComponentTestAdapter = imageGameObjectTestAdapter.AddImageComponent();
            imageComponentTestAdapter.Sprite = sprite;

            var tooltipAdapter = imageComponentTestAdapter.AddTooltipMouseOverComponent();
            var newParent = imageGameObjectTestAdapter.Transform.Parent.Parent;
            tooltipAdapter.Transform.SetParent(newParent);

            return imageGameObjectTestAdapter;
        }
    }
}
