// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using Customizing;
using JetBrains.Annotations;
using LobotomyCorporationMods.FreeCustomization;
using LobotomyCorporationMods.Test.Extensions;

#endregion

namespace LobotomyCorporationMods.Test.FreeCustomizationTests
{
    [SuppressMessage("ReSharper", "Unity.IncorrectMonoBehaviourInstantiation")]
    public class FreeCustomizationTests
    {
        protected FreeCustomizationTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.LoadData(mockLogger.Object);
        }

        [NotNull]
        protected static CustomizingWindow GetCustomizingWindow()
        {
            // Need a WorkerSpriteManager instance
            InitializeWorkerSpriteManager();

            return TestExtensions.CreateCustomizingWindow();
        }

        protected static void InitializeAgentInfoWindow()
        {
            _ = TestExtensions.CreateAgentInfoWindow();
        }

        private static void InitializeWorkerSpriteManager()
        {
            _ = TestExtensions.CreateWorkerSpriteManager();
        }
    }
}
