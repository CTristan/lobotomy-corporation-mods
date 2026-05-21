// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.ObjectTests
{
    public sealed class SerializablePresetListTests
    {
        [Fact]
        public void ToPresetList_converts_each_entry_to_PresetData()
        {
            var sut = new SerializablePresetList();
            sut.Presets["Alice"] = BuildRawPresetEntry("eyeBattle");

            var presetList = sut.ToPresetList();

            presetList.Presets.Should().ContainKey("Alice");
            presetList.Presets["Alice"].EyeBattle.Should().Be("eyeBattle");
        }

        [Fact]
        public void ToPresetList_maps_every_field_from_the_raw_dictionary()
        {
            var sut = new SerializablePresetList();
            sut.Presets["Alice"] = BuildRawPresetEntry("tag");

            var data = sut.ToPresetList().Presets["Alice"];

            data.EyeBattle.Should().Be("tag");
            data.EyebrowBattle.Should().Be("tag");
            data.EyebrowDef.Should().Be("tag");
            data.EyebrowPanic.Should().Be("tag");
            data.EyeDef.Should().Be("tag");
            data.EyePanic.Should().Be("tag");
            data.EyeDead.Should().Be("tag");
            data.RearHair.Should().Be("tag");
            data.FrontHair.Should().Be("tag");
            data.MouthBattle.Should().Be("tag");
            data.MouthDef.Should().Be("tag");
            data.MouthPanic.Should().Be("tag");
            data.HairColor.Should().Be("tag");
            data.EyeColor.Should().Be("tag");
        }

        [Fact]
        public void ToPresetList_throws_when_entry_is_not_a_dictionary()
        {
            var sut = new SerializablePresetList();
            sut.Presets["Alice"] = "not a dictionary";

            Action act = () => sut.ToPresetList();

            act.Should().Throw<ArgumentException>();
        }

        private static Dictionary<string, object> BuildRawPresetEntry(string value)
        {
            return new Dictionary<string, object>
            {
                { nameof(PresetData.EyeBattle), value },
                { nameof(PresetData.EyebrowBattle), value },
                { nameof(PresetData.EyebrowDef), value },
                { nameof(PresetData.EyebrowPanic), value },
                { nameof(PresetData.EyeDef), value },
                { nameof(PresetData.EyePanic), value },
                { nameof(PresetData.EyeDead), value },
                { nameof(PresetData.RearHair), value },
                { nameof(PresetData.FrontHair), value },
                { nameof(PresetData.MouthBattle), value },
                { nameof(PresetData.MouthDef), value },
                { nameof(PresetData.MouthPanic), value },
                { nameof(PresetData.HairColor), value },
                { nameof(PresetData.EyeColor), value },
            };
        }
    }
}
