// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.ObjectTests
{
    public sealed class PresetListTests
    {
        [Fact]
        public void ToJson_returns_empty_object_when_no_presets()
        {
            var presetList = new PresetList();

            var json = presetList.ToJson();

            json.Should().Be($"{{{Environment.NewLine}{Environment.NewLine}}}");
        }

        [Fact]
        public void ToJson_renders_single_preset_under_its_key()
        {
            var presetList = new PresetList();
            presetList.Presets["Alice"] = new PresetData { EyeBattle = "eb" };

            var json = presetList.ToJson();

            json.Should().Contain("\"Alice\":");
            json.Should().Contain("\"EyeBattle\": \"eb\"");
        }

        [Fact]
        public void ToJson_separates_multiple_presets_with_commas()
        {
            var presetList = new PresetList();
            presetList.Presets["Alice"] = new PresetData { EyeBattle = "a" };
            presetList.Presets["Bob"] = new PresetData { EyeBattle = "b" };

            var json = presetList.ToJson();

            json.Should().Contain("\"Alice\":");
            json.Should().Contain("\"Bob\":");
            json.Should().Contain(",");
        }
    }
}
