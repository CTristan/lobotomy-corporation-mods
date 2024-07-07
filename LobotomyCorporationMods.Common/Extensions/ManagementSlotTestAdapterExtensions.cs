// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ManagementSlotTestAdapterExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this IManagementSlotTestAdapter managementSlotTestAdapter,
            float localScaleX,
            float localScaleY,
            float localPositionX,
            float localPositionY,
            float localPositionZ,
            [NotNull] ITexture2dTestAdapter texture2dTestAdapter,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            imageGameObjectTestAdapter = imageGameObjectTestAdapter.EnsureNotNullWithMethod(() => new GameObjectTestAdapter(new GameObject()));
            spriteTestAdapter = spriteTestAdapter.EnsureNotNullWithMethod(() => new SpriteTestAdapter(new Sprite()));

            var parent = managementSlotTestAdapter.Transform.GetChild(0);
            imageGameObjectTestAdapter.Transform.SetParent(parent);
            imageGameObjectTestAdapter.Transform.LocalScale = new Vector3(localScaleX, localScaleY);
            imageGameObjectTestAdapter.Transform.LocalPosition = new Vector3(localPositionX, localPositionY, localPositionZ);
            imageGameObjectTestAdapter.SetActive(true);

            var sprite = spriteTestAdapter.Create(texture2dTestAdapter.GameObject, new Rect(0f, 0f, texture2dTestAdapter.Width, texture2dTestAdapter.Height), new Vector2(0.5f, 0.5f));

            var imageComponentTestAdapter = imageGameObjectTestAdapter.AddImageComponent();
            imageComponentTestAdapter.Sprite = sprite;

            var tooltipAdapter = imageComponentTestAdapter.AddTooltipMouseOverComponent();
            var newParent = imageGameObjectTestAdapter.Transform.Parent.Parent;
            tooltipAdapter.Transform.SetParent(newParent);

            return imageGameObjectTestAdapter;
        }
    }
}
