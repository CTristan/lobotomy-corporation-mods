// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Xml.Linq;

namespace CI
{
    internal interface ICoverageThresholdChecker
    {
        bool CheckThresholds(string repoRoot, out string failureMessage);
    }

    internal sealed class CoverageThresholdChecker(IFileSystem fileSystem) : ICoverageThresholdChecker
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
            List<string> coverageReports = FindCoverageReports(repoRoot);

            if (coverageReports.Count == 0)
            {
                failureMessage = "No coverage reports found. Ensure tests are run with coverage collection enabled.";
                return false;
            }

            try
            {
                List<ModuleCoverage> moduleCoverages = GetModuleCoverages(coverageReports);

                if (moduleCoverages.Count == 0)
                {
                    failureMessage = "No coverage metrics found in reports.";
                    return false;
                }

                // Print overall coverage for information
                CoverageTotals overallCoverage = CalculateOverallCoverage(moduleCoverages);
                Console.WriteLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "=== Coverage results: Line {0:F1}%, Branch {1:F1}%, Method {2:F1}% ===",
                    overallCoverage.LineCoverage,
                    overallCoverage.BranchCoverage,
                    overallCoverage.MethodCoverage));

                // Now check against thresholds from coverlet.json
                string configPath = System.IO.Path.Combine(repoRoot, "coverlet.json");

                if (!_fileSystem.FileExists(configPath))
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                string? json = _fileSystem.ReadAllText(configPath);

                if (json == null)
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                CoverageConfig? config = JsonSerializer.Deserialize<CoverageConfig>(json);

                if (config == null)
                {
                    failureMessage = "Coverlet config not found";
                    return false;
                }

                List<string> failures = [];

                foreach (ModuleCoverage module in moduleCoverages)
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
            catch (Exception ex)
            {
                failureMessage = $"Error processing coverage reports: {ex.Message}";
                return false;
            }
        }

        private static List<string> FindCoverageReports(string repoRoot)
        {
            List<string> reports = [];
            string[] testDirectories = System.IO.Directory.GetDirectories(repoRoot, "*.Test");

            foreach (string testDir in testDirectories)
            {
                string reportPath = System.IO.Path.Combine(testDir, "coverage.opencover.xml");

                if (System.IO.File.Exists(reportPath))
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
                   "HarmonyDebugPanel" or
                   "LobotomyPlaywright" or
                   "LobotomyPlaywright.Plugin";
        }

        private static ModuleClassSummary GetClassSummary(XElement class_, string namespaceName)
        {
            XElement? classSummary = class_.Element(XName.Get("Summary", namespaceName));

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

            IEnumerable<XElement> classes = module.Descendants(XName.Get("Class", namespaceName));

            foreach (XElement class_ in classes)
            {
                ModuleClassSummary summary = GetClassSummary(class_, namespaceName);
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
            List<ModuleCoverage> moduleCoverages = [];

            foreach (string reportPath in coverageReports)
            {
                string xml = System.IO.File.ReadAllText(reportPath);
                XDocument doc = XDocument.Parse(xml);
                XNamespace? ns = doc.Root?.GetDefaultNamespace();
                string namespaceName = ns?.NamespaceName ?? string.Empty;

                IEnumerable<XElement> modules = doc.Descendants(XName.Get("Module", namespaceName));

                foreach (XElement module in modules)
                {
                    XElement? moduleNameElement = module.Element(XName.Get("ModuleName", namespaceName));
                    string moduleName = moduleNameElement?.Value ?? "Unknown";

                    if (ShouldSkipModule(moduleName))
                    {
                        continue;
                    }

                    ModuleCoverage moduleCoverage = CreateModuleCoverage(moduleName, module, namespaceName);
                    if (moduleCoverage != null)
                    {
                        moduleCoverages.Add(moduleCoverage);
                    }
                }
            }

            return moduleCoverages;
        }

        private static CoverageTotals CalculateOverallCoverage(List<ModuleCoverage> moduleCoverages)
        {
            double totalLineVisited = 0;
            double totalLineCovered = 0;
            double totalBranchVisited = 0;
            double totalBranchCovered = 0;
            double totalMethodVisited = 0;
            double totalMethodCovered = 0;

            foreach (ModuleCoverage module in moduleCoverages)
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

    internal sealed class ModuleCoverage
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

    internal sealed class CoverageTotals
    {
        public double LineCoverage { get; init; }
        public double BranchCoverage { get; init; }
        public double MethodCoverage { get; init; }
    }

    internal sealed class ModuleClassSummary
    {
        public double LineVisited { get; init; }
        public double LineCovered { get; init; }
        public double BranchVisited { get; init; }
        public double BranchCovered { get; init; }
        public double MethodVisited { get; init; }
        public double MethodCovered { get; init; }
    }
}
