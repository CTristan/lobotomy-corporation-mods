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
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using UnityEngine;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter")]
    public static class ManagementUiFacade
    {
        [ThreadStatic] private static Dictionary<string, IImageTestAdapter> s_imagesDictionary;

        private static void CreateImageObjectIfNotExist([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            Guard.Against.Null(managementSlot, nameof(managementSlot));
            Guard.Against.Null(fileManager, nameof(fileManager));

            s_imagesDictionary = s_imagesDictionary.EnsureNotNullWithMethod(() => new Dictionary<string, IImageTestAdapter>());

            // When retrieving the object from the dictionary, it is possible that Unity has destroyed the engine object.
            if (s_imagesDictionary.TryGetValue(imageName, out var value) && !value.IsUnityNull())
            {
                return;
            }

            const float LocalPositionX = -12f;
            const float LocalPositionY = 28f;
            const float LocalPositionZ = -1f;
            const float LocalScaleX = 0.2f;
            const float LocalScaleY = 0.2f;

            texture2dTestAdapter = texture2dTestAdapter.EnsureNotNullWithMethod(() => new Texture2dTestAdapter(new Texture2D(2, 2)));

            var fileWithPath = fileManager.GetFile(imagePath);
            if (string.IsNullOrEmpty(fileWithPath))
            {
                throw new InvalidOperationException("No image found with name " + imagePath);
            }

            texture2dTestAdapter.LoadImage(fileManager.ReadAllBytes(fileWithPath));

            var imageObject = managementSlot.CreateImageObjectTestAdapter(LocalScaleX, LocalScaleY, LocalPositionX, LocalPositionY, LocalPositionZ, texture2dTestAdapter, testAdapter,
                imageGameObjectTestAdapter, spriteTestAdapter);

            var imageTestAdapter = imageObject.ImageComponent;
            s_imagesDictionary[imageName] = imageTestAdapter;
        }

        public static string GetSlotName([NotNull] this ManagementSlot managementSlot,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null)
        {
            testAdapter = testAdapter.EnsureNotNullWithMethod(() => new ManagementSlotTestAdapter(managementSlot));

            return testAdapter.Name;
        }

        public static void HideImageObject([NotNull] this ManagementSlot managementSlot,
            [NotNull] string imageName,
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            CreateImageObjectIfNotExist(managementSlot, imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
            var image = GetImage(imageName);

            image.Color = Color.clear;

            var tooltip = image.TooltipMouseOverComponent;
            tooltip.SetActive(false);
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
            [NotNull] string imagePath,
            [NotNull] IFileManager fileManager,
            Color color,
            [CanBeNull] string tooltipMessage = "",
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null,
            [CanBeNull] IGameObjectTestAdapter imageGameObjectTestAdapter = null,
            [CanBeNull] ITexture2dTestAdapter texture2dTestAdapter = null,
            [CanBeNull] ISpriteTestAdapter spriteTestAdapter = null)
        {
            CreateImageObjectIfNotExist(managementSlot, imageName, imagePath, fileManager, testAdapter, imageGameObjectTestAdapter, texture2dTestAdapter, spriteTestAdapter);
            var image = GetImage(imageName);
            image.Color = color;

            var tooltip = image.TooltipMouseOverComponent;
            tooltip.SetActive(true);
            tooltip.SetDynamicTooltip(tooltipMessage);
        }

        [NotNull]
        private static IImageTestAdapter GetImage([NotNull] string imageName)
        {
            return s_imagesDictionary[imageName];
        }
    }
}
