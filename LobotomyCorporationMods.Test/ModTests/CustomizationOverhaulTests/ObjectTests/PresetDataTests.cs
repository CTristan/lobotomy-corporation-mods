// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using Customizing;
using LobotomyCorporationMods.CustomizationOverhaul.Objects;
using LobotomyCorporationMods.Test.Extensions;
using UnityEngine;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CustomizationOverhaulTests.ObjectTests
{
    public sealed class PresetDataTests
    {
        [Fact]
        public void FromAppearanceData_throws_when_appearance_is_null()
        {
            Action act = () => PresetData.FromAppearanceData(null);

            act.Should().Throw<ArgumentNullException>();
        }

        // Sprite.name goes through UnityEngine.Object.get_name() which is an ECall backed by
        // the native Unity runtime and cannot be invoked outside the game. So we only exercise
        // the null-sprite branches here; the non-null branch is exercised in-game.
        [Fact]
        public void FromAppearanceData_converts_colors_to_html_strings()
        {
            var appearance = new Appearance
            {
                spriteSet = new WorkerSprite.WorkerSprite(),
                HairColor = Color.red,
                EyeColor = Color.blue,
            };

            var data = PresetData.FromAppearanceData(appearance);

            data.HairColor.Should().Be(ColorUtility.ToHtmlStringRGB(Color.red));
            data.EyeColor.Should().Be(ColorUtility.ToHtmlStringRGB(Color.blue));
        }

        [Fact]
        public void FromAppearanceData_yields_null_sprite_names_when_sprites_are_null()
        {
            var appearance = new Appearance
            {
                spriteSet = new WorkerSprite.WorkerSprite(),
                HairColor = Color.white,
                EyeColor = Color.white,
            };

            var data = PresetData.FromAppearanceData(appearance);

            data.EyeBattle.Should().BeNull();
            data.FrontHair.Should().BeNull();
            data.MouthDef.Should().BeNull();
            data.RearHair.Should().BeNull();
        }

        [Fact]
        public void ToJson_emits_each_property_on_its_own_line()
        {
            var data = new PresetData
            {
                EyeBattle = "eb",
                EyeDead = "ed",
                EyeDef = "edf",
                EyePanic = "ep",
                EyebrowBattle = "ebb",
                EyebrowDef = "ebd",
                EyebrowPanic = "ebp",
                FrontHair = "fh",
                MouthBattle = "mb",
                MouthDef = "md",
                MouthPanic = "mp",
                RearHair = "rh",
                HairColor = "FFFFFF",
                EyeColor = "000000",
            };

            var json = data.ToJson();

            json.Should().StartWith("{").And.EndWith("}");
            json.Should().Contain("\"EyeBattle\": \"eb\"");
            json.Should().Contain("\"EyeColor\": \"000000\"");
        }

        [Fact]
        public void ToJson_indents_each_line_by_two_spaces_per_level()
        {
            var data = new PresetData { EyeBattle = "eb" };

            var json = data.ToJson(2);

            json.Should().Contain("    \"EyeBattle\": \"eb\"");
            json.Should().StartWith("    {");
        }
    }
}
