// SPDX-License-Identifier: MIT

using System;
using CommandWindow;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Interfaces;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.ParameterObjects;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedParameter.Global

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class ManagementUiFacade
    {
        [ThreadStatic] private static UnityAdapterDictionary<string, IImageTestAdapter, Image> s_imagesDictionary;

        private static void CreateImageObjectIfNotExist([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            _ = Guard.Against.Null(managementSlot, nameof(managementSlot));
            _ = Guard.Against.Null(fileManager, nameof(fileManager));

            s_imagesDictionary = s_imagesDictionary.EnsureNotNullWithMethod(() => new UnityAdapterDictionary<string, IImageTestAdapter, Image>());
            testAdapterParameters = testAdapterParameters.EnsureNotNullWithMethod(() => new OptionalTestAdapterParameters());
            testAdapterParameters.Texture2DTestAdapter = testAdapterParameters.Texture2DTestAdapter.EnsureNotNullWithMethod(() => new Texture2dTestAdapter());

            if (s_imagesDictionary.ContainsKey(imageParameters.ImageId))
            {
                return;
            }

            string fileWithPath = fileManager.GetFile(imageParameters.ImageFilePath);
            if (string.IsNullOrEmpty(fileWithPath))
            {
                throw new InvalidOperationException("No image found with name " + imageParameters.ImageFilePath);
            }

            _ = testAdapterParameters.Texture2DTestAdapter.LoadImage(fileManager.ReadAllBytes(fileWithPath));

            IGameObjectTestAdapter imageObject = managementSlot.CreateImageObjectTestAdapter(imageParameters, testAdapterParameters);

            IImageTestAdapter imageTestAdapter = imageObject.ImageComponent;
            s_imagesDictionary[imageParameters.ImageId] = imageTestAdapter;
        }

        public static string GetSlotName([NotNull] this ManagementSlot managementSlot,
            [CanBeNull] IManagementSlotTestAdapter testAdapter = null)
        {
            testAdapter = testAdapter.EnsureNotNullWithMethod(() => new ManagementSlotTestAdapter(managementSlot));

            return testAdapter.Name;
        }

        public static void HideImageObject([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            _ = Guard.Against.Null(imageParameters, nameof(imageParameters));

            CreateImageObjectIfNotExist(managementSlot, imageParameters, fileManager, testAdapterParameters);
            IImageTestAdapter image = GetImage(imageParameters.ImageId);

            image.Color = Color.clear;

            ITooltipMouseOverTestAdapter tooltip = image.TooltipMouseOverComponent;
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
            _ = Guard.Against.Null(commandWindow, nameof(commandWindow));

            // Validation checks to confirm we have everything we need
            bool isAbnormalityWorkWindow = commandWindow.CurrentSkill.IsNotNull() && commandWindow.CurrentWindowType == CommandType.Management;

            return isAbnormalityWorkWindow;
        }

        public static void UpdateAgentSlot([NotNull] this AgentSlot agentSlot,
            Color slotColor,
            string slotText,
            [CanBeNull] IImageTestAdapter imageTestAdapter = null,
            [CanBeNull] ITextTestAdapter textTestAdapter = null)
        {
            _ = Guard.Against.Null(agentSlot, nameof(agentSlot));

            imageTestAdapter = imageTestAdapter.EnsureNotNullWithMethod(() => new ImageTestAdapter(agentSlot.WorkFilterFill));
            textTestAdapter = textTestAdapter.EnsureNotNullWithMethod(() => new TextTestAdapter(agentSlot.WorkFilterText));

            imageTestAdapter.Color = slotColor;
            textTestAdapter.Text = slotText;
            agentSlot.SetColor(slotColor);
        }

        public static void UpdateImage([NotNull] this ManagementSlot managementSlot,
            [NotNull] ImageParameters imageParameters,
            [NotNull] IFileManager fileManager,
            Color color,
            [CanBeNull] string tooltipMessage = "",
            [CanBeNull] OptionalTestAdapterParameters testAdapterParameters = null)
        {
            _ = Guard.Against.Null(imageParameters, nameof(imageParameters));

            CreateImageObjectIfNotExist(managementSlot, imageParameters, fileManager, testAdapterParameters);

            IImageTestAdapter image = GetImage(imageParameters.ImageId);
            image.Color = color;
            image.SetActive(true);

            ITooltipMouseOverTestAdapter tooltip = image.TooltipMouseOverComponent;
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
