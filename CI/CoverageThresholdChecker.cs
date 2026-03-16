// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;

namespace CI
{
    public interface ICoverageThresholdChecker
    {
        bool CheckThresholds(string repoRoot, out string failureMessage);
    }

    public sealed class CoverageThresholdChecker(IFileSystem fileSystem) : ICoverageThresholdChecker
    {
        private readonly IFileSystem _fileSystem = fileSystem;

        public CoverageThresholdChecker()
            : this(new FileSystem())
        {
        }

        public bool CheckThresholds(string repoRoot, out string failureMessage)
        {
            failureMessage = string.Empty;

            // Find all coverage.opencover.xml files in subdirectories
            var coverageReports = FindCoverageReports(repoRoot);

            if (coverageReports.Count == 0)
            {
                failureMessage = "No coverage reports found. Ensure tests are run with coverage collection enabled.";
                return false;
            }

            try
            {
                var moduleCoverages = GetModuleCoverages(coverageReports);

                if (moduleCoverages.Count == 0)
                {
                    failureMessage = "No coverage metrics found in reports.";
                    return false;
                }

                // Print overall coverage for information
                var overallCoverage = CalculateOverallCoverage(moduleCoverages);
                Console.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "=== Coverage results: Line {0:F1}%, Branch {1:F1}%, Method {2:F1}% ===",
                    overallCoverage.LineCoverage,
                    overallCoverage.BranchCoverage,
                    overallCoverage.MethodCoverage));

                // Now check against thresholds from coverlet.json
                var configPath = Path.Combine(repoRoot, "coverlet.json");

                if (!_fileSystem.FileExists(configPath))
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                var json = _fileSystem.ReadAllText(configPath);

                if (json == null)
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                var config = JsonSerializer.Deserialize<CoverageConfig>(json);

                if (config == null)
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                List<string> failures = [];

                foreach (var module in moduleCoverages)
                {
                    if (module.Totals.LineCoverage < config.LineThreshold)
                    {
                        failures.Add(string.Format(
                            CultureInfo.InvariantCulture,
                            "Module '{0}' line coverage ({1:F1}%) is below threshold ({2:F1}%)",
                            module.ModuleName,
                            module.Totals.LineCoverage,
                            config.LineThreshold));
                    }

                    if (module.Totals.BranchCoverage < config.BranchThreshold)
                    {
                        failures.Add(string.Format(
                            CultureInfo.InvariantCulture,
                            "Module '{0}' branch coverage ({1:F1}%) is below threshold ({2:F1}%)",
                            module.ModuleName,
                            module.Totals.BranchCoverage,
                            config.BranchThreshold));
                    }

                    if (module.Totals.MethodCoverage < config.MethodThreshold)
                    {
                        failures.Add(string.Format(
                            CultureInfo.InvariantCulture,
                            "Module '{0}' method coverage ({1:F1}%) is below threshold ({2:F1}%)",
                            module.ModuleName,
                            module.Totals.MethodCoverage,
                            config.MethodThreshold));
                    }
                }

                if (failures.Count > 0)
                {
                    failureMessage = string.Join(Environment.NewLine, failures);
                    return false;
                }

                return true;
            }
            catch (IOException ex)
            {
                failureMessage = $"Error reading coverage files: {ex.Message}";
                return false;
            }
            catch (JsonException ex)
            {
                failureMessage = $"Error parsing coverlet configuration: {ex.Message}";
                return false;
            }
            catch (System.Xml.XmlException ex)
            {
                failureMessage = $"Error parsing coverage XML: {ex.Message}";
                return false;
            }
            catch (InvalidOperationException ex)
            {
                failureMessage = $"Error processing coverage data: {ex.Message}";
                return false;
            }
            catch (FormatException ex)
            {
                failureMessage = $"Error parsing numeric values in coverage data: {ex.Message}";
                return false;
            }
        }

        private static List<string> FindCoverageReports(string repoRoot)
        {
            List<string> reports = [];
            var testDirectories = Directory.GetDirectories(repoRoot, "*.Test");

            foreach (var testDir in testDirectories)
            {
                var reportPath = Path.Combine(testDir, "coverage.opencover.xml");

                if (File.Exists(reportPath))
                {
                    reports.Add(reportPath);
                }
            }

            return reports;
        }

        private static bool ShouldSkipModule(string moduleName)
        {
            // Skip the test projects themselves if they appear as modules
            if (moduleName.EndsWith(".Test", StringComparison.OrdinalIgnoreCase) ||
                moduleName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Skip local dotnet tools (executables) from coverage thresholds
            // These are CLI tools with different testing requirements than mod libraries
            return moduleName is "CI" or
                   "SetupExternal" or
                   "RetargetHarmony" or
                   "RetargetHarmony.Installer" or
                   "HarmonyDebugPanel" or
                   "LobotomyPlaywright" or
                   "LobotomyPlaywright.Plugin";
        }

        private static ModuleClassSummary GetClassSummary(XElement class_, string namespaceName)
        {
            var classSummary = class_.Element(XName.Get("Summary", namespaceName));

            return classSummary == null
                ? new ModuleClassSummary()
                : new ModuleClassSummary
                {
                    LineVisited = (double?)classSummary.Attribute("numSequencePoints") ?? 0,
                    LineCovered = (double?)classSummary.Attribute("visitedSequencePoints") ?? 0,
                    BranchVisited = (double?)classSummary.Attribute("numBranchPoints") ?? 0,
                    BranchCovered = (double?)classSummary.Attribute("visitedBranchPoints") ?? 0,
                    MethodVisited = (double?)classSummary.Attribute("numMethods") ?? 0,
                    MethodCovered = (double?)classSummary.Attribute("visitedMethods") ?? 0,
                };
        }

        private static ModuleCoverage CreateModuleCoverage(string moduleName, XElement module, string namespaceName)
        {
            double totalLineVisited = 0;
            double totalLineCovered = 0;
            double totalBranchVisited = 0;
            double totalBranchCovered = 0;
            double totalMethodVisited = 0;
            double totalMethodCovered = 0;

            var classes = module.Descendants(XName.Get("Class", namespaceName));

            foreach (var class_ in classes)
            {
                var summary = GetClassSummary(class_, namespaceName);
                totalLineVisited += summary.LineVisited;
                totalLineCovered += summary.LineCovered;
                totalBranchVisited += summary.BranchVisited;
                totalBranchCovered += summary.BranchCovered;
                totalMethodVisited += summary.MethodVisited;
                totalMethodCovered += summary.MethodCovered;
            }

            // Only add modules that actually have code
            return totalLineVisited > 0
                ? new ModuleCoverage
                {
                    ModuleName = moduleName,
                    LineVisited = totalLineVisited,
                    LineCovered = totalLineCovered,
                    BranchVisited = totalBranchVisited,
                    BranchCovered = totalBranchCovered,
                    MethodVisited = totalMethodVisited,
                    MethodCovered = totalMethodCovered,
                    Totals = new CoverageTotals
                    {
                        LineCoverage = totalLineCovered / totalLineVisited * 100,
                        BranchCoverage = totalBranchVisited == 0 ? 100.0 : totalBranchCovered / totalBranchVisited * 100,
                        MethodCoverage = totalMethodVisited == 0 ? 100.0 : totalMethodCovered / totalMethodVisited * 100,
                    }
                }
                : null!;
        }

        private static List<ModuleCoverage> GetModuleCoverages(List<string> coverageReports)
        {
            Dictionary<string, ModuleCoverage> bestCoverages = new(StringComparer.OrdinalIgnoreCase);

            foreach (var reportPath in coverageReports)
            {
                var xml = File.ReadAllText(reportPath);
                XDocument doc = XDocument.Parse(xml);
                var ns = doc.Root?.GetDefaultNamespace();
                var namespaceName = ns?.NamespaceName ?? string.Empty;

                var modules = doc.Descendants(XName.Get("Module", namespaceName));

                foreach (var module in modules)
                {
                    var moduleNameElement = module.Element(XName.Get("ModuleName", namespaceName));
                    var moduleName = moduleNameElement?.Value ?? "Unknown";

                    if (ShouldSkipModule(moduleName))
                    {
                        continue;
                    }

                    var moduleCoverage = CreateModuleCoverage(moduleName, module, namespaceName);
                    if (moduleCoverage != null)
                    {
                        // When the same module appears in multiple test projects,
                        // keep the entry with the highest line coverage
                        if (!bestCoverages.TryGetValue(moduleName, out var existing) ||
                            moduleCoverage.Totals.LineCoverage > existing.Totals.LineCoverage)
                        {
                            bestCoverages[moduleName] = moduleCoverage;
                        }
                    }
                }
            }

            return [.. bestCoverages.Values];
        }

        private static CoverageTotals CalculateOverallCoverage(List<ModuleCoverage> moduleCoverages)
        {
            double totalLineVisited = 0;
            double totalLineCovered = 0;
            double totalBranchVisited = 0;
            double totalBranchCovered = 0;
            double totalMethodVisited = 0;
            double totalMethodCovered = 0;

            foreach (var module in moduleCoverages)
            {
                totalLineVisited += module.LineVisited;
                totalLineCovered += module.LineCovered;
                totalBranchVisited += module.BranchVisited;
                totalBranchCovered += module.BranchCovered;
                totalMethodVisited += module.MethodVisited;
                totalMethodCovered += module.MethodCovered;
            }

            return new CoverageTotals
            {
                LineCoverage = totalLineVisited == 0 ? 100.0 : totalLineCovered / totalLineVisited * 100,
                BranchCoverage = totalBranchVisited == 0 ? 100.0 : totalBranchCovered / totalBranchVisited * 100,
                MethodCoverage = totalMethodVisited == 0 ? 100.0 : totalMethodCovered / totalMethodVisited * 100,
            };
        }
    }

    public sealed class ModuleCoverage
    {
        public string ModuleName { get; init; } = string.Empty;
        public double LineVisited { get; init; }
        public double LineCovered { get; init; }
        public double BranchVisited { get; init; }
        public double BranchCovered { get; init; }
        public double MethodVisited { get; init; }
        public double MethodCovered { get; init; }
        public CoverageTotals Totals { get; init; } = new CoverageTotals();
    }

    public sealed class CoverageTotals
    {
        public double LineCoverage { get; init; }
        public double BranchCoverage { get; init; }
        public double MethodCoverage { get; init; }
    }

    public sealed class ModuleClassSummary
    {
        public double LineVisited { get; init; }
        public double LineCovered { get; init; }
        public double BranchVisited { get; init; }
        public double BranchCovered { get; init; }
        public double MethodVisited { get; init; }
        public double MethodCovered { get; init; }
    }
}
