// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.Mods.FreeCustomization
{
    public class FreeCustomizationTests
    {
        protected FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        protected static AgentInfoWindow InitializeAgentInfoWindow()
        {
            return TestExtensions.CreateAgentInfoWindow();
        }

        protected static CustomizingWindow InitializeCustomizingWindow(CustomizingType currentWindowType = CustomizingType.GENERATE)
        {
            return InitializeCustomizingWindow(null, currentWindowType);
        }

        protected static CustomizingWindow InitializeCustomizingWindow(AgentModel currentAgent, CustomizingType currentWindowType = CustomizingType.GENERATE)
        {
            // Need a WorkerSpriteManager instance
            InitializeWorkerSpriteManager();

            return TestExtensions.CreateCustomizingWindow(currentAgent: currentAgent, currentWindowType: currentWindowType);
        }

        private static void InitializeWorkerSpriteManager()
        {
            _ = TestExtensions.CreateWorkerSpriteManager();
        }
    }
}
