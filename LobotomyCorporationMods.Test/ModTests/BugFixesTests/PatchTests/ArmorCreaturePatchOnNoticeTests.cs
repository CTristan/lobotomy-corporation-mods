// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using Hemocode.BugFixes.Patches;
using LobotomyCorporation.Mods.Common.Interfaces.Adapters;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests.PatchTests
{
    public sealed class ArmorCreaturePatchOnNoticeTests : BugFixesModTests
    {
        private readonly object[] _defaultParams = new object[1];
        private readonly Mock<IArmorCreatureTestAdapter> _mockArmorCreatureTestAdapter = new();
        private readonly string _onChangeGiftNotice = NoticeName.OnChangeGift;

        public ArmorCreaturePatchOnNoticeTests()
        {
            _ = _mockArmorCreatureTestAdapter.Setup(a => a.SpecialAgentList).Returns(new List<object>());
        }

        [Fact]
        public void Any_agent_changing_their_gift_causes_Crumbling_Armor_list_to_reset()
        {
            // Act
            ArmorCreature.PatchAfterOnNotice(_onChangeGiftNotice, _mockArmorCreatureTestAdapter.Object, _defaultParams);

            // Assert
            VerifyListReset();
        }

        [Theory]
        [InlineData(nameof(NoticeName.OnWorkStart))]
        [InlineData(nameof(NoticeName.OnReleaseWork))]
        public void Receiving_any_other_notice_does_not_cause_Crumbling_Armor_list_to_reset([NotNull] string notice)
        {
            // Act
            ArmorCreature.PatchAfterOnNotice(notice, _mockArmorCreatureTestAdapter.Object, _defaultParams);

            // Assert
            VerifyListNotReset();
        }

        #region Helper Methods

        private void VerifyListReset()
        {
            _mockArmorCreatureTestAdapter.Verify(x => x.ReloadSpecialAgentList(), Times.Once);
        }

        private void VerifyListNotReset()
        {
            _mockArmorCreatureTestAdapter.Verify(x => x.ReloadSpecialAgentList(), Times.Never);
        }

        #endregion
    }
}
