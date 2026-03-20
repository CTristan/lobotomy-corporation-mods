// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace Harmony2ForLmm.Models
{
    /// <summary>
    /// Source-generated JSON serializer context for trimming compatibility.
    /// </summary>
    [JsonSerializable(typeof(InstallationManifest))]
    [JsonSourceGenerationOptions(WriteIndented = true)]
    internal sealed partial class ManifestJsonContext : JsonSerializerContext;
}
