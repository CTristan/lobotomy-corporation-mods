// SPDX-License-Identifier: MIT

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.Common.Extensions;
using LobotomyCorporationMods.Common.Implementations.Adapters;
using LobotomyCorporationMods.Common.Implementations.Adapters.BaseClasses;
using LobotomyCorporationMods.Common.Interfaces.Adapters;
using LobotomyCorporationMods.Common.Interfaces.Adapters.BaseClasses;

namespace LobotomyCorporationMods.Common.Implementations.Facades
{
    public static class CustomizeAgentUiFacade
    {
        public static void LoadAgentData([NotNull] this CustomizingWindow customizingWindow,
            [NotNull] AgentModel agent)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));
            Guard.Against.Null(agent, nameof(agent));

            customizingWindow.CurrentData.agentName = agent._agentName;
            customizingWindow.CurrentData.CustomName = agent.name;
            customizingWindow.CurrentData.appearance = agent.GetAppearanceData();
        }

        public static void OpenAppearancePanel([NotNull] this AgentInfoWindow agentInfoWindow,
            IAgentInfoWindowUiComponentsTestAdapter uiComponentsTestAdapter = null,
            ICustomizingWindowTestAdapter customizingWindowTestAdapter = null,
            IGameObjectTestAdapter gameObjectTestAdapter = null)
        {
            Guard.Against.Null(agentInfoWindow, nameof(agentInfoWindow));

            gameObjectTestAdapter = gameObjectTestAdapter.EnsureNotNullWithMethod(() => new GameObjectTestAdapter(agentInfoWindow.customizingBlock));
            uiComponentsTestAdapter = uiComponentsTestAdapter.EnsureNotNullWithMethod(() => new AgentInfoWindowUiComponentsTestAdapter(agentInfoWindow.UIComponents));
            customizingWindowTestAdapter = customizingWindowTestAdapter.EnsureNotNullWithMethod(() => new CustomizingWindowTestAdapter(agentInfoWindow.customizingWindow));

            var customizingWindow = CustomizingWindow.CurrentWindow;

            // Make sure the customizing block is active so that we can customize the agent
            gameObjectTestAdapter.GameObject = agentInfoWindow.customizingBlock;
            gameObjectTestAdapter.SetActive(true);

            // Make the appearance control active
            gameObjectTestAdapter.GameObject = agentInfoWindow.AppearanceActiveControl;
            gameObjectTestAdapter.SetActive(true);

            uiComponentsTestAdapter.SetData(customizingWindow.CurrentData);
            customizingWindowTestAdapter.OpenAppearanceWindow();
        }

        public static void SaveAppearanceData([NotNull] this CustomizingWindow customizingWindow,
            IAgentLayerTestAdapter agentLayerTestAdapter = null,
            IWorkerSpriteManagerTestAdapter workerSpriteManagerTestAdapter = null)
        {
            Guard.Against.Null(customizingWindow, nameof(customizingWindow));

            agentLayerTestAdapter = agentLayerTestAdapter.EnsureNotNullWithMethod(() => new AgentLayerTestAdapter(AgentLayer.currentLayer));
            workerSpriteManagerTestAdapter = workerSpriteManagerTestAdapter.EnsureNotNullWithMethod(() => new WorkerSpriteManagerTestAdapter(WorkerSpriteManager.instance));

            // The original method does this and other things for newly-generated agents, so we only want to do this
            // when we're updating existing agents.
            if (customizingWindow.CurrentWindowType == CustomizingType.GENERATE)
            {
                return;
            }

            customizingWindow.SaveAgentAppearance();
            customizingWindow.RenameAgent();
            customizingWindow.CurrentData.appearance.SetResrouceData();

            workerSpriteManagerTestAdapter.SetAgentBasicData(customizingWindow.CurrentData.appearance.spriteSet, customizingWindow.CurrentData.appearance);
            customizingWindow.UpdateAgentModel(agentLayerTestAdapter);
        }

        public static void UpdateAgentStats(this CustomizingWindow customizingWindow,
            [NotNull] AgentModel agent,
            [NotNull] AgentData agentData,
            ICustomizingWindowTestAdapter customizingWindowTestAdapter = null)
        {
            Guard.Against.Null(agent, nameof(agent));
            Guard.Against.Null(agentData, nameof(agentData));
            customizingWindowTestAdapter = customizingWindowTestAdapter.EnsureNotNullWithMethod(() => new CustomizingWindowTestAdapter(customizingWindow));

            agent.primaryStat.hp = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.hp, agent.originFortitudeLevel, agentData.statBonus.rBonus);
            agent.primaryStat.mental = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.mental, agent.originPrudenceLevel, agentData.statBonus.wBonus);
            agent.primaryStat.work = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.work, agent.originTemperanceLevel, agentData.statBonus.bBonus);
            agent.primaryStat.battle = customizingWindowTestAdapter.SetRandomStatValue(agent.primaryStat.battle, agent.originJusticeLevel, agentData.statBonus.pBonus);
            agent.UpdateTitle(agent.level);
        }
    }
}
