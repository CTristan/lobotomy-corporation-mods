// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using AwesomeAssertions;
using LobotomyCorporationMods.Common.Enums.Diagnostics;
using LobotomyCorporationMods.Common.Models.Diagnostics;
using LobotomyCorporationMods.Playwright.JsonModels.Diagnostics;
using Xunit;

#endregion

namespace LobotomyCorporationMods.Playwright.Test.JsonModels
{
    public sealed class DiagnosticsDataTests
    {
        [Fact]
        public void AssemblyInfoData_FromModel_maps_all_fields()
        {
            var model = new AssemblyInfo("TestAssembly", "1.0.0", "/path/to/test.dll", true);

            var data = AssemblyInfoData.FromModel(model);

            data.name.Should().Be("TestAssembly");
            data.version.Should().Be("1.0.0");
            data.location.Should().Be("/path/to/test.dll");
            data.isHarmonyRelated.Should().BeTrue();
        }

        [Fact]
        public void AssemblyInfoData_FromModel_throws_when_model_is_null()
        {
            Action act = () => AssemblyInfoData.FromModel(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AssemblyInfoData_FromModels_maps_all_entries()
        {
            var models = new List<AssemblyInfo>
            {
                new("Assembly1", "1.0", "/path1", false),
                new("Assembly2", "2.0", "/path2", true)
            };

            var result = AssemblyInfoData.FromModels(models);

            result.Should().HaveCount(2);
            result[0].name.Should().Be("Assembly1");
            result[1].name.Should().Be("Assembly2");
        }

        [Fact]
        public void AssemblyInfoData_FromModels_throws_when_models_is_null()
        {
            Action act = () => AssemblyInfoData.FromModels(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DetectedModData_FromModel_maps_all_fields()
        {
            var model = new DetectedModInfo(
                "TestMod", "1.0", ModSource.BepInExPlugin,
                HarmonyVersion.Harmony2, "TestAssembly", "com.test.mod",
                true, 5, 10);

            var data = DetectedModData.FromModel(model);

            data.name.Should().Be("TestMod");
            data.version.Should().Be("1.0");
            data.source.Should().Be("BepInExPlugin");
            data.harmonyVersion.Should().Be("Harmony2");
            data.assemblyName.Should().Be("TestAssembly");
            data.identifier.Should().Be("com.test.mod");
        }

        [Fact]
        public void DetectedModData_FromModel_throws_when_model_is_null()
        {
            Action act = () => DetectedModData.FromModel(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DetectedModData_FromModels_maps_all_entries()
        {
            var models = new List<DetectedModInfo>
            {
                new("Mod1", "1.0", ModSource.BepInExPlugin, HarmonyVersion.Harmony1, "Asm1", "id1", false, 0, 0),
                new("Mod2", "2.0", ModSource.Lmm, HarmonyVersion.Harmony2, "Asm2", "id2", true, 3, 5)
            };

            var result = DetectedModData.FromModels(models);

            result.Should().HaveCount(2);
            result[0].name.Should().Be("Mod1");
            result[1].name.Should().Be("Mod2");
        }

        [Fact]
        public void DetectedModData_FromModels_throws_when_models_is_null()
        {
            Action act = () => DetectedModData.FromModels(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DllIntegrityFindingData_FromModel_maps_all_fields()
        {
            var model = new DllIntegrityFinding(
                "/path/test.dll", "test.dll", FindingSeverity.Warning,
                ["0Harmony"], ["Harmony"],
                true, "/backup/test.dll", true, "Rewritten");

            var data = DllIntegrityFindingData.FromModel(model);

            data.dllPath.Should().Be("/path/test.dll");
            data.dllName.Should().Be("test.dll");
            data.severity.Should().Be("Warning");
            data.onDiskHarmonyReferences.Should().HaveCount(1);
            data.originalHarmonyReferences.Should().HaveCount(1);
            data.hasBackup.Should().BeTrue();
            data.backupPath.Should().Be("/backup/test.dll");
            data.wasRewritten.Should().BeTrue();
            data.summary.Should().Be("Rewritten");
        }

        [Fact]
        public void DllIntegrityFindingData_FromModel_throws_when_model_is_null()
        {
            Action act = () => DllIntegrityFindingData.FromModel(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DllIntegrityFindingData_FromModels_maps_all_entries()
        {
            var models = new List<DllIntegrityFinding>
            {
                new("/path1", "dll1", FindingSeverity.Info, [], [], false, string.Empty, false, "OK")
            };

            var result = DllIntegrityFindingData.FromModels(models);

            result.Should().HaveCount(1);
            result[0].dllName.Should().Be("dll1");
        }

        [Fact]
        public void DllIntegrityFindingData_FromModels_throws_when_models_is_null()
        {
            Action act = () => DllIntegrityFindingData.FromModels(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void MissingPatchData_FromModel_maps_all_fields()
        {
            var model = new MissingPatchInfo(
                "TestAssembly", "TestClass", "TestMethod",
                "PatchMethod", PatchType.Postfix);

            var data = MissingPatchData.FromModel(model);

            data.patchAssembly.Should().Be("TestAssembly");
            data.targetType.Should().Be("TestClass");
            data.targetMethod.Should().Be("TestMethod");
            data.patchMethod.Should().Be("PatchMethod");
            data.patchType.Should().Be("Postfix");
        }

        [Fact]
        public void MissingPatchData_FromModel_throws_when_model_is_null()
        {
            Action act = () => MissingPatchData.FromModel(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void MissingPatchData_FromModels_maps_all_entries()
        {
            var models = new List<MissingPatchInfo>
            {
                new("Asm1", "Type1", "Method1", "Patch1", PatchType.Prefix),
                new("Asm2", "Type2", "Method2", "Patch2", PatchType.Postfix)
            };

            var result = MissingPatchData.FromModels(models);

            result.Should().HaveCount(2);
            result[0].patchAssembly.Should().Be("Asm1");
            result[1].patchAssembly.Should().Be("Asm2");
        }

        [Fact]
        public void MissingPatchData_FromModels_throws_when_models_is_null()
        {
            Action act = () => MissingPatchData.FromModels(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PatchInfoData_FromModel_maps_all_fields()
        {
            var model = new PatchInfo(
                "TargetType", "TargetMethod", PatchType.Prefix,
                "Owner", "PatchMethod", "PatchAssembly");

            var data = PatchInfoData.FromModel(model);

            data.targetType.Should().Be("TargetType");
            data.targetMethod.Should().Be("TargetMethod");
            data.patchType.Should().Be("Prefix");
            data.owner.Should().Be("Owner");
            data.patchMethod.Should().Be("PatchMethod");
            data.patchAssemblyName.Should().Be("PatchAssembly");
        }

        [Fact]
        public void PatchInfoData_FromModel_throws_when_model_is_null()
        {
            Action act = () => PatchInfoData.FromModel(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PatchInfoData_FromModels_maps_all_entries()
        {
            var models = new List<PatchInfo>
            {
                new("Type1", "Method1", PatchType.Prefix, "Owner1", "Patch1", "Asm1")
            };

            var result = PatchInfoData.FromModels(models);

            result.Should().HaveCount(1);
            result[0].targetType.Should().Be("Type1");
        }

        [Fact]
        public void PatchInfoData_FromModels_throws_when_models_is_null()
        {
            Action act = () => PatchInfoData.FromModels(null);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
