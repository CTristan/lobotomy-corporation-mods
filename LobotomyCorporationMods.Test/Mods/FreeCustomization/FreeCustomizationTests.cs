// SPDX-License-Identifier: MIT

#region

using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization
{
    public class FreeCustomizationTests
    {
        private const AgentModel DefaultAgentModel = null;
        private const CustomizingType DefaultCustomizingType = CustomizingType.GENERATE;

        protected FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        [NotNull]
        protected static AgentInfoWindow InitializeAgentInfoWindow()
        {
            return UnityTestExtensions.CreateAgentInfoWindow();
        }

        [NotNull]
        protected static CustomizingWindow InitializeCustomizingWindow(CustomizingType currentWindowType)
        {
            return InitializeCustomizingWindow(DefaultAgentModel, currentWindowType);
        }

        [NotNull]
        protected static CustomizingWindow InitializeCustomizingWindow([CanBeNull] AgentModel currentAgent = DefaultAgentModel,
            CustomizingType currentWindowType = DefaultCustomizingType)
        {
            // Need a WorkerSpriteManager instance
            InitializeWorkerSpriteManager();

            return UnityTestExtensions.CreateCustomizingWindow(currentAgent: currentAgent, currentWindowType: currentWindowType);
        }

        private static void InitializeWorkerSpriteManager()
        {
            _ = UnityTestExtensions.CreateWorkerSpriteManager();
        }
    }
}
