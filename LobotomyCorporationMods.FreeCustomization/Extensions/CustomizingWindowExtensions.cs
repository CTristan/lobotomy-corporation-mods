// SPDX-License-Identifier: MIT

#region

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Interfaces.Adapters;

#endregion

namespace LobotomyCorporationMods.FreeCustomization.Extensions
{
    internal static class CustomizingWindowExtensions
    {
        internal static void RenameAgent([NotNull] this CustomizingWindow customizingWindow)
        {
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
            if (customizingWindow.appearanceUI.copied.IsNotNull())
            {
                customizingWindow.CurrentData.AppearCopy(customizingWindow.appearanceUI.copied);
                customizingWindow.appearanceUI.copied = null;
            }

            customizingWindow.CurrentAgent.SetAppearanceData(customizingWindow.CurrentData.appearance);
        }

        internal static void UpdateAgentModel([NotNull] this CustomizingWindow customizingWindow,
            [NotNull] IAgentLayerTestAdapter agentLayerTestAdapter)
        {
            var agentModel = customizingWindow.CurrentAgent;
            agentLayerTestAdapter.RemoveAgent(agentModel);
            agentLayerTestAdapter.AddAgent(agentModel);
        }
    }
}
