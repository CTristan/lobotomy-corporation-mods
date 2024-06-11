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

        protected static CustomizingWindow InitializeCustomizingWindow()
        {
            return InitializeCustomizingWindow(null);
        }

        protected static CustomizingWindow InitializeCustomizingWindow(AgentModel currentAgent)
        {
            // Need a WorkerSpriteManager instance
            InitializeWorkerSpriteManager();

            return TestExtensions.CreateCustomizingWindow(currentAgent: currentAgent);
        }

        private static void InitializeWorkerSpriteManager()
        {
            _ = TestExtensions.CreateWorkerSpriteManager();
        }
    }
}
