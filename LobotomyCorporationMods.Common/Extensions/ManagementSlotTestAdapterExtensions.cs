// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Hemocode.Common.Implementations.Adapters;
using Hemocode.Common.Implementations.Adapters.BaseClasses;
using Hemocode.Common.Interfaces.Adapters;
using Hemocode.Common.Interfaces.Adapters.BaseClasses;
using Hemocode.Common.ParameterObjects;
using UnityEngine;

namespace Hemocode.Common.Extensions
{
    public static class ManagementSlotTestAdapterExtensions
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

            var imageGameObjectTestAdapter = testAdapterParameters.GameObjectTestAdapter;
            var parent = managementSlotTestAdapter.Transform.GetChild(0);
            imageGameObjectTestAdapter.Transform.SetParent(parent);
            imageGameObjectTestAdapter.Transform.LocalScale = new Vector3(imageParameters.LocalScaleX, imageParameters.LocalScaleY);
            imageGameObjectTestAdapter.Transform.LocalPosition = new Vector3(imageParameters.LocalPositionX, imageParameters.LocalPositionY, imageParameters.LocalPositionZ);
            imageGameObjectTestAdapter.SetActive(true);

            var texture2dTestAdapter = testAdapterParameters.Texture2DTestAdapter;
            var sprite = testAdapterParameters.SpriteTestAdapter.Create(texture2dTestAdapter.GameObject, new Rect(0f, 0f, texture2dTestAdapter.Width, texture2dTestAdapter.Height),
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
