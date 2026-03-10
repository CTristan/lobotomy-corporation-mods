// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace CI
{
    public class CoverageConfig
    {
        [JsonPropertyName("lineThreshold")]
        public double LineThreshold { get; set; } = 80;

        [JsonPropertyName("branchThreshold")]
        public double BranchThreshold { get; set; } = 70;

        [JsonPropertyName("methodThreshold")]
        public double MethodThreshold { get; set; } = 75;
    }
}
