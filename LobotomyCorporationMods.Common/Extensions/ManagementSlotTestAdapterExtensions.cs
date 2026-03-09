// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterObjects;
using UnityEngine;

namespace LobotomyCorporationMods.Common.Extensions
{
    internal static class ManagementSlotTestAdapterExtensions
    {
        [NotNull]
        internal static IGameObjectTestAdapter CreateImageObjectTestAdapter([NotNull] this IManagementSlotTestAdapter managementSlotTestAdapter,
            [NotNull] ImageParameters imageParameters,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            testAdapterParameters = testAdapterParameters.EnsureNotNullWithMethod(() => new OptionalTestAdapterParameters());
            testAdapterParameters.SpriteTestAdapter = testAdapterParameters.SpriteTestAdapter.EnsureNotNullWithMethod(() => new SpriteTestAdapter(new Sprite()));
            testAdapterParameters.Texture2DTestAdapter = testAdapterParameters.Texture2DTestAdapter.EnsureNotNullWithMethod(() => new Texture2dTestAdapter());
            testAdapterParameters.GameObjectTestAdapter = testAdapterParameters.GameObjectTestAdapter.EnsureNotNullWithMethod(() => new GameObjectTestAdapter(new GameObject()));

            IGameObjectTestAdapter imageGameObjectTestAdapter = testAdapterParameters.GameObjectTestAdapter;
            ITransformTestAdapter parent = managementSlotTestAdapter.Transform.GetChild(0);
            imageGameObjectTestAdapter.Transform.SetParent(parent);
            imageGameObjectTestAdapter.Transform.LocalScale = new Vector3(imageParameters.LocalScaleX, imageParameters.LocalScaleY);
            imageGameObjectTestAdapter.Transform.LocalPosition = new Vector3(imageParameters.LocalPositionX, imageParameters.LocalPositionY, imageParameters.LocalPositionZ);
            imageGameObjectTestAdapter.SetActive(true);

            ITexture2dTestAdapter texture2dTestAdapter = testAdapterParameters.Texture2DTestAdapter;
            Sprite sprite = testAdapterParameters.SpriteTestAdapter.Create(texture2dTestAdapter.GameObject, new Rect(0f, 0f, texture2dTestAdapter.Width, texture2dTestAdapter.Height),
                new Vector2(0.5f, 0.5f));

            IImageTestAdapter imageComponentTestAdapter = imageGameObjectTestAdapter.AddImageComponent();
            imageComponentTestAdapter.Sprite = sprite;

            ITooltipMouseOverTestAdapter tooltipAdapter = imageComponentTestAdapter.AddTooltipMouseOverComponent();
            ITransformTestAdapter newParent = imageGameObjectTestAdapter.Transform.Parent.Parent;
            tooltipAdapter.Transform.SetParent(newParent);

            return imageGameObjectTestAdapter;
        }
    }
}
