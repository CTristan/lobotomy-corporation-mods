// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.DebugPanel.Implementations;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DebugPanelTests
{
    public sealed class GameplayLogErrorParserTests
    {
        [Fact]
        public void Parse_returns_empty_list_for_null_content()
        {
            var parser = new GameplayLogErrorParser();

            var result = parser.Parse(null);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Parse_returns_empty_list_for_empty_content()
        {
            var parser = new GameplayLogErrorParser();

            var result = parser.Parse(string.Empty);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Parse_returns_empty_list_when_no_herror_lines()
        {
            var parser = new GameplayLogErrorParser();

            var result = parser.Parse("Normal log line\nAnother line");

            result.Should().BeEmpty();
        }

        [Fact]
        public void Parse_extracts_mod_name_dll_name_and_error_message()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - Skin Tone Customization v0.1.0 / SkinTone.dllException has been thrown by the target of an invocation.";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].ModName.Should().Be("Skin Tone Customization v0.1.0");
            result[0].DllName.Should().Be("SkinTone.dll");
            result[0].ErrorMessage.Should().Be("Exception has been thrown by the target of an invocation.");
            result[0].RawLine.Should().Be(logContent);
        }

        [Fact]
        public void Parse_handles_multiple_herror_entries()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - Mod1 / Mod1.dllError1" + Environment.NewLine +
                             "Herror - Mod2 / Mod2.dllError2";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(2);
            result[0].ModName.Should().Be("Mod1");
            result[1].ModName.Should().Be("Mod2");
        }

        [Fact]
        public void Parse_collects_stack_trace_lines()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / Test.dllSomeError" + Environment.NewLine +
                             "  at System.Reflection.MethodBase.Invoke()" + Environment.NewLine +
                             "  at HarmonyLib.PatchProcessor.Patch()";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].StackTrace.Should().Contain("System.Reflection.MethodBase.Invoke()");
            result[0].StackTrace.Should().Contain("HarmonyLib.PatchProcessor.Patch()");
        }

        [Fact]
        public void Parse_stops_collecting_stack_trace_at_non_indented_line()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / Test.dllSomeError" + Environment.NewLine +
                             "  at System.Reflection.MethodBase.Invoke()" + Environment.NewLine +
                             "Normal log line after stack trace";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].StackTrace.Should().Contain("System.Reflection.MethodBase.Invoke()");
            result[0].StackTrace.Should().NotContain("Normal log line");
        }

        [Fact]
        public void Parse_handles_missing_separator()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - some raw error without separator";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].ModName.Should().BeEmpty();
            result[0].DllName.Should().BeEmpty();
            result[0].ErrorMessage.Should().Be("some raw error without separator");
            result[0].RawLine.Should().Be(logContent);
        }

        [Fact]
        public void Parse_handles_missing_dll_extension()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / NoDllExtensionHere";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].ModName.Should().Be("TestMod");
            result[0].DllName.Should().BeEmpty();
            result[0].ErrorMessage.Should().Be("NoDllExtensionHere");
        }

        [Fact]
        public void Parse_returns_empty_stack_trace_when_no_indented_lines_follow()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / Test.dllSomeError" + Environment.NewLine +
                             "Normal next line";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].StackTrace.Should().BeEmpty();
        }

        [Fact]
        public void Parse_ignores_non_herror_lines_between_entries()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Some log line" + Environment.NewLine +
                             "Herror - Mod1 / Mod1.dllError1" + Environment.NewLine +
                             "Another log line" + Environment.NewLine +
                             "Herror - Mod2 / Mod2.dllError2";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(2);
        }

        [Fact]
        public void Parse_handles_crlf_line_endings()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / Test.dllSomeError\r\n  at Method()\r\nNormal line";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].StackTrace.Should().Contain("Method()");
        }

        [Fact]
        public void Parse_handles_herror_at_end_of_file_without_newline()
        {
            var parser = new GameplayLogErrorParser();
            var logContent = "Herror - TestMod / Test.dllSomeError";

            var result = parser.Parse(logContent);

            result.Should().HaveCount(1);
            result[0].ErrorMessage.Should().Be("SomeError");
        }
    }
}
