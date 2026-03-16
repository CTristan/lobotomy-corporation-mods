// SPDX-License-Identifier: MIT

#region

using AwesomeAssertions;
using LobotomyCorporationMods.Playwright.Queries;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.Queries
{
    public sealed class DataClassTests
    {
        [Fact]
        public void AgentData_properties_are_settable_and_gettable()
        {
            var data = new AgentData
            {
                InstanceId = 1,
                Name = "TestAgent",
                Hp = 100f,
                MaxHp = 100f,
                Mental = 80f,
                MaxMental = 100f,
                Fortitude = 5,
                Prudence = 4,
                Temperance = 3,
                Justice = 2,
                CurrentSefira = "MALKUTH",
                State = "IDLE",
                GiftIds = ["g1", "g2"],
                WeaponId = "weapon1",
                ArmorId = "armor1",
                IsDead = false,
                IsPanicking = false
            };

            data.InstanceId.Should().Be(1);
            data.Name.Should().Be("TestAgent");
            data.Hp.Should().Be(100f);
            data.MaxHp.Should().Be(100f);
            data.Mental.Should().Be(80f);
            data.MaxMental.Should().Be(100f);
            data.Fortitude.Should().Be(5);
            data.Prudence.Should().Be(4);
            data.Temperance.Should().Be(3);
            data.Justice.Should().Be(2);
            data.CurrentSefira.Should().Be("MALKUTH");
            data.State.Should().Be("IDLE");
            data.GiftIds.Should().HaveCount(2);
            data.WeaponId.Should().Be("weapon1");
            data.ArmorId.Should().Be("armor1");
            data.IsDead.Should().BeFalse();
            data.IsPanicking.Should().BeFalse();
        }

        [Fact]
        public void CreatureData_properties_are_settable_and_gettable()
        {
            var data = new CreatureData
            {
                InstanceId = 10,
                MetadataId = 100,
                Name = "TestCreature",
                RiskLevel = "WAW",
                State = "NORMAL",
                QliphothCounter = 3,
                MaxQliphothCounter = 5,
                FeelingState = "GOOD",
                CurrentSefira = "YESOD",
                ObservationLevel = 2,
                WorkCount = 10,
                IsEscaping = false,
                IsSuppressed = false
            };

            data.InstanceId.Should().Be(10);
            data.MetadataId.Should().Be(100);
            data.Name.Should().Be("TestCreature");
            data.RiskLevel.Should().Be("WAW");
            data.State.Should().Be("NORMAL");
            data.QliphothCounter.Should().Be(3);
            data.MaxQliphothCounter.Should().Be(5);
            data.FeelingState.Should().Be("GOOD");
            data.CurrentSefira.Should().Be("YESOD");
            data.ObservationLevel.Should().Be(2);
            data.WorkCount.Should().Be(10);
            data.IsEscaping.Should().BeFalse();
            data.IsSuppressed.Should().BeFalse();
        }

        [Fact]
        public void SefiraData_properties_are_settable_and_gettable()
        {
            var data = new SefiraData
            {
                Name = "Malkuth",
                SefiraEnum = "MALKUTH",
                IsOpen = true,
                OpenLevel = 3,
                AgentIds = [1, 2, 3],
                CreatureIds = [10, 20],
                OfficerCount = 5
            };

            data.Name.Should().Be("Malkuth");
            data.SefiraEnum.Should().Be("MALKUTH");
            data.IsOpen.Should().BeTrue();
            data.OpenLevel.Should().Be(3);
            data.AgentIds.Should().HaveCount(3);
            data.CreatureIds.Should().HaveCount(2);
            data.OfficerCount.Should().Be(5);
        }

        [Fact]
        public void UiNodeData_properties_are_settable_and_gettable()
        {
            var data = new UiNodeData
            {
                Path = "Panel/TitleText",
                Type = "text",
                Value = "Hello",
                Interactable = false
            };

            data.Path.Should().Be("Panel/TitleText");
            data.Type.Should().Be("text");
            data.Value.Should().Be("Hello");
            data.Interactable.Should().BeFalse();
        }

        [Fact]
        public void UiStateData_properties_are_settable_and_gettable()
        {
            var data = new UiStateData
            {
                Windows = [new() { Name = "TestWindow" }],
                ActivatedSlots = ["slot1"],
                ModElements = [new() { Path = "mod/element" }]
            };

            data.Windows.Should().HaveCount(1);
            data.ActivatedSlots.Should().HaveCount(1);
            data.ModElements.Should().HaveCount(1);
        }

        [Fact]
        public void UiWindowData_properties_are_settable_and_gettable()
        {
            var data = new UiWindowData
            {
                Name = "AgentInfoWindow",
                IsOpen = true,
                WindowType = "info",
                Children = [new() { Path = "child" }]
            };

            data.Name.Should().Be("AgentInfoWindow");
            data.IsOpen.Should().BeTrue();
            data.WindowType.Should().Be("info");
            data.Children.Should().HaveCount(1);
        }
    }
}
