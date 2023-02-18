// SPDX-License-Identifier: MIT

#region

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    internal static class CustomizingWindowExtensions
    {
        internal static void RenameAgent([NotNull] this CustomizingWindow customizingWindow)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));

            var customName = customizingWindow.CurrentData.CustomName;
            customizingWindow.CurrentAgent.name = customName;
            customizingWindow.CurrentAgent._agentName = customizingWindow.CurrentData.agentName;
            customizingWindow.CurrentAgent.iscustom = true;

            customizingWindow.CurrentAgent._agentName.metaInfo.nameDic.Clear();
            customizingWindow.CurrentAgent._agentName.nameDic.Clear();

            foreach (var language in SupportedLanguage.GetSupprotedList())
            {
                customizingWindow.CurrentAgent._agentName.metaInfo.nameDic.Add(language, customName);
                customizingWindow.CurrentAgent._agentName.nameDic.Add(language, customName);
            }
        }

        internal static void SaveAgentAppearance([NotNull] this CustomizingWindow customizingWindow)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));

            if (customizingWindow.appearanceUI.copied is not null)
            {
                customizingWindow.CurrentData.AppearCopy(customizingWindow.appearanceUI.copied);
                customizingWindow.appearanceUI.copied = null;
            }

            customizingWindow.CurrentAgent.SetAppearanceData(customizingWindow.CurrentData.appearance);
        }

        internal static void UpdateAgentModel([NotNull] this CustomizingWindow customizingWindow, [NotNull] IAgentLayerAdapter agentLayerAdapter)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));

            var agentModel = customizingWindow.CurrentAgent;
            agentLayerAdapter.RemoveAgent(agentModel);
            agentLayerAdapter.AddAgent(agentModel);
        }
    }
}
