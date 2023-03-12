// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace LobotomyCorporationMods.GiftAvailabilityIndicator.Extensions
{
    internal static class AdapterExtensions
    {
        internal static IGameObjectAdapter CreateImageGameObject(this IAdapter<Component> component, IGameObjectAdapter imageGameObject, IFileManager fileManager, ITexture2DAdapter texture2DAdapter,
            ISpriteAdapter spriteAdapter, IImageAdapter imageAdapter)
        {
            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;

            imageGameObject.Transform.SetParent(component.GameObjectAdapter.Transform.GetChild(0));
            imageGameObject.Transform.LocalScale = new Vector3(LocalScaleX, LocalScaleY);
            imageGameObject.Transform.LocalPosition = new Vector3(LocalPositionX, LocalPositionY, LocalPositionZ);
            imageGameObject.SetActive(true);

            var fileWithPath = fileManager.GetFile("Assets/gift.png");
            texture2DAdapter.LoadImage(fileManager.ReadAllBytes(fileWithPath));
            var sprite = spriteAdapter.Create(texture2DAdapter.GameObject, new Rect(0f, 0f, texture2DAdapter.Width, texture2DAdapter.Height), new Vector2(0.5f, 0.5f));

            imageAdapter.GameObject = imageGameObject.AddComponent<Image>();
            imageAdapter.Sprite = sprite;

            var tooltipAdapter = imageAdapter.GameObjectAdapter.AddComponentAdapter<TooltipMouseOver>();
            var newParent = imageGameObject.Transform.ParentAdapter.Parent;
            tooltipAdapter.GameObjectAdapter.Transform.SetParent(newParent);

            return imageGameObject;
        }
    }
}
