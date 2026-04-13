// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using JetBrains.Annotations;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.BugFixes.Patches;
using Moq;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.BugFixesTests.PatchTests
{
    public sealed class ArmorCreaturePatchOnNoticeTests : BugFixesModTests
    {
        private readonly object[] _defaultParams = new object[1];
        private readonly Mock<IArmorCreatureInternals> _mockArmorCreatureInternals =
            new Mock<IArmorCreatureInternals>();
        private readonly string _onChangeGiftNotice = NoticeName.OnChangeGift;

        public ArmorCreaturePatchOnNoticeTests()
        {
            _mockArmorCreatureInternals.Setup(a => a.SpecialAgentList).Returns(new List<object>());
        }

        [Fact]
        public void Any_agent_changing_their_gift_causes_Crumbling_Armor_list_to_reset()
        {
            // Act
            ArmorCreature.PatchAfterOnNotice(
                _onChangeGiftNotice,
                _mockArmorCreatureInternals.Object,
                _defaultParams
            );

            // Assert
            VerifyListReset();
        }

        [Theory]
        [InlineData(nameof(NoticeName.OnWorkStart))]
        [InlineData(nameof(NoticeName.OnReleaseWork))]
        public void Receiving_any_other_notice_does_not_cause_Crumbling_Armor_list_to_reset(
            [NotNull] string notice
        )
        {
            // Act
            ArmorCreature.PatchAfterOnNotice(
                notice,
                _mockArmorCreatureInternals.Object,
                _defaultParams
            );

            // Assert
            VerifyListNotReset();
        }

        #region Helper Methods

        private void VerifyListReset()
        {
            _mockArmorCreatureInternals.Verify(x => x.ReloadSpecialAgentList(), Times.Once);
        }

        private void VerifyListNotReset()
        {
            _mockArmorCreatureInternals.Verify(x => x.ReloadSpecialAgentList(), Times.Never);
        }

        #endregion
    }
}
