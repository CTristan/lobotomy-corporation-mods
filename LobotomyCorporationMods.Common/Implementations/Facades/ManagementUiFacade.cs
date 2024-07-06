// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public static class ManagementUiFacade
    {
        [ThreadStatic] private static Dictionary<string, Image> s_imagesDictionary;

        public static void CreateImageObjectIfNotExist([NotNull] this ManagementSlot instance,
            [NotNull] string imageName,
            [NotNull] IFileManager fileManager)
        {
            Guard.Against.Null(instance, nameof(instance));
            Guard.Against.Null(fileManager, nameof(fileManager));

            s_imagesDictionary = s_imagesDictionary.EnsureNotNullWithMethod(() => new Dictionary<string, Image>());
            if (s_imagesDictionary.ContainsKey(imageName))
            {
                return;
            }

            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;
            const string ImagePath = "Assets/gift.png";

            var imageObject = new GameObject();
            var texture2dObject = new Texture2D(0, 0);

            imageObject.transform.SetParent(instance.transform.GetChild(0));
            imageObject.transform.localScale = new Vector3(LocalScaleX, LocalScaleY);
            imageObject.transform.localPosition = new Vector3(LocalPositionX, LocalPositionY, LocalPositionZ);
            imageObject.SetActive(true);

            var fileWithPath = fileManager.GetOrCreateFile(ImagePath, false);
            if (string.IsNullOrEmpty(fileWithPath))
            {
                throw new InvalidOperationException("No image found with name " + ImagePath);
            }

            texture2dObject.LoadImage(fileManager.ReadAllBytes(fileWithPath));
            var sprite = Sprite.Create(texture2dObject, new Rect(0f, 0f, texture2dObject.width, texture2dObject.height), new Vector2(0.5f, 0.5f));

            var imageComponent = imageObject.AddComponent<Image>();
            imageComponent.sprite = sprite;

            var tooltipAdapter = imageComponent.gameObject.AddComponent<TooltipMouseOver>();
            var newParent = imageObject.transform.parent;
            tooltipAdapter.gameObject.transform.SetParent(newParent);

            s_imagesDictionary.Add(imageName, imageObject.GetComponent<Image>());
        }

        public static void HideImageObject([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName)
        {
            var image = GetImage(imageName);

            image.color = Color.clear;

            var tooltip = image.GetComponent<TooltipMouseOver>();
            tooltip.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Determines whether the current CommandWindow is an abnormality work window. An abnormality work window is a CommandWindow in the Management phase with a non-null rwbpType
        ///     in the CurrentSkill property.
        /// </summary>
        /// <param name="commandWindow">The current CommandWindow to check.</param>
        /// <returns><c>true</c> if the current CommandWindow is an abnormality work window, otherwise <c>false</c>.</returns>
        public static bool IsAbnormalityWorkWindow([NotNull] this CommandWindow.CommandWindow commandWindow)
        {
            Guard.Against.Null(commandWindow, nameof(commandWindow));

            // Validation checks to confirm we have everything we need
            var isAbnormalityWorkWindow = commandWindow.CurrentSkill.IsNotNull() && commandWindow.CurrentWindowType == CommandType.Management;

            return isAbnormalityWorkWindow;
        }

        public static void UpdateAgentSlot([NotNull] this AgentSlot agentSlot,
            Color slotColor,
            string slotText,
            [CanBeNull] IImageTestAdapter imageTestAdapter = null,
            [CanBeNull] ITextTestAdapter textTestAdapter = null)
        {
            Guard.Against.Null(agentSlot, nameof(agentSlot));

            imageTestAdapter = imageTestAdapter.EnsureNotNullWithMethod(() => new ImageTestAdapter(agentSlot.WorkFilterFill));
            textTestAdapter = textTestAdapter.EnsureNotNullWithMethod(() => new TextTestAdapter(agentSlot.WorkFilterText));

            imageTestAdapter.Color = slotColor;
            textTestAdapter.Text = slotText;
            agentSlot.SetColor(slotColor);
        }

        public static void UpdateImage([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            Color color,
            [CanBeNull] string tooltip = null)
        {
            var image = GetImage(imageName);
            image.color = color;

            if (tooltip.IsNull())
            {
                return;
            }

            var tooltipAdapter = image.GetComponent<TooltipMouseOver>();
            tooltipAdapter.SetDynamicTooltip(tooltip);
        }

        [NotNull]
        private static Image GetImage([NotNull] string imageName)
        {
            s_imagesDictionary.TryGetValue(imageName, out var image);
            if (image.IsNull())
            {
                throw new InvalidOperationException("No image found with name " + imageName);
            }

            return image;
        }
    }
}
