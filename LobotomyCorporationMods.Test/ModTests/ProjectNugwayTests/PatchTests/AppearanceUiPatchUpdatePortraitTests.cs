// SPDX-License-Identifier: MIT

#region

using Customizing;
using LobotomyCorporationMods.ProjectNugway.Interfaces;
using LobotomyCorporationMods.ProjectNugway.Patches;
using LobotomyCorporationMods.Test.Extensions;
using Moq;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.ProjectNugwayTests.PatchTests
{
    public sealed class AppearanceUiPatchUpdatePortraitTests : ProjectNugwayModTests
    {
        private readonly Mock<IUiController> _uiControllerMock = new Mock<IUiController>();
        private AppearanceUI _sut;

        [Theory]
        [InlineData("AgentName")]
        [InlineData("a")]
        public void Display_save_preset_button_when_AgentInfoWindow_is_open(string agentName)
        {
            UnityTestExtensions.CreateAgentInfoWindow();
            var globalGameManager = UnityTestExtensions.CreateGlobalGameManager();
            globalGameManager.Language = SystemLanguage.English;

            var nameInput = UnityTestExtensions.CreateInputField(agentName);
            _sut = UnityTestExtensions.CreateAppearanceUi(nameInput);

            // Current agent data has no name, so will fail the test if the Name Input is empty
            _sut.copied = new AgentData();

            _sut.PatchAfterUpdatePortrait(_uiControllerMock.Object);

            _uiControllerMock.Verify(ui => ui.DisplaySavePresetButton(), Times.Once);
            _uiControllerMock.Verify(ui => ui.UpdateSavePresetButtonText(agentName, It.IsAny<Appearance>()), Times.Once);
        }

        [Theory]
        [InlineData("AgentName")]
        [InlineData("a")]
        public void Use_current_agent_name_when_name_text_box_is_empty(string agentName)
        {
            UnityTestExtensions.CreateAgentInfoWindow();
            var globalGameManager = UnityTestExtensions.CreateGlobalGameManager();
            globalGameManager.Language = SystemLanguage.English;

            // Empty name text box
            var nameInput = UnityTestExtensions.CreateInputField();

            var agentNameTypeInfo = new AgentNameTypeInfo();
            agentNameTypeInfo.nameDic.Add("en", agentName);
            _sut = UnityTestExtensions.CreateAppearanceUi(nameInput);
            _sut.copied = new AgentData
            {
                agentName = new AgentName(agentNameTypeInfo, 0),
            };

            _sut.PatchAfterUpdatePortrait(_uiControllerMock.Object);

            _uiControllerMock.Verify(ui => ui.DisplaySavePresetButton(), Times.Once);
            _uiControllerMock.Verify(ui => ui.UpdateSavePresetButtonText(agentName, It.IsAny<Appearance>()), Times.Once);
        }

        [Theory]
        [InlineData("AgentName")]
        [InlineData("a")]
        public void Does_not_display_UI_components_if_AgentInfoWindow_is_not_instantiated(string agentName)
        {
            AgentInfoWindow.currentWindow = null;
            var globalGameManager = UnityTestExtensions.CreateGlobalGameManager();
            globalGameManager.Language = SystemLanguage.English;
            var nameInput = UnityTestExtensions.CreateInputField(agentName);
            _sut = UnityTestExtensions.CreateAppearanceUi(nameInput);
            _sut.copied = new AgentData();

            _sut.PatchAfterUpdatePortrait(_uiControllerMock.Object);

            _uiControllerMock.Verify(ui => ui.DisplaySavePresetButton(), Times.Never);
            _uiControllerMock.Verify(ui => ui.UpdateSavePresetButtonText(agentName, It.IsAny<Appearance>()), Times.Never);
        }
    }
}
