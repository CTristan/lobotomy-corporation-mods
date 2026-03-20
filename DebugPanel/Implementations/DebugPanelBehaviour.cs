// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Common.Implementations;
using DebugPanel.Interfaces;
using DebugPanel.JsonModels;
using DebugPanel.Common.Models.Diagnostics;
using UnityEngine;

#endregion

namespace DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class DebugPanelBehaviour : MonoBehaviour
    {
        private DebugPanelConfig _config;
        private IDiagnosticReportBuilder _reportBuilder;
        private IOverlayRenderer _overlay;
        private IInputHandler _inputHandler;
        private ILogFileWriter _logFileWriter;
        private DiagnosticReport _report;
        private bool _isOverlayVisible = true;
        private float _startTime;
        private int _frameCount;

        private void Awake()
        {
            try
            {
                DontDestroyOnLoad(gameObject);

                var fileManager = Harmony_Patch.Instance.FileManager;
                var configProvider = new ConfigProvider(fileManager);
                _config = configProvider.LoadConfig();

                var detector = new EnvironmentDetector();
                var harmony1Source = new Harmony1PatchInspectionSource();
                var harmony2Source = new Harmony2PatchInspectionSource();
                var assemblySource = new AppDomainAssemblyInspectionSource();
                var pluginInfoSource = new ReflectionBepInExPluginInfoSource();
                var classifier = new HarmonyVersionClassifier();

                var basicDllInspector = new BasicDllFileInspector(new PeAssemblyRefReader());
                var cecilDllInspector = new CecilDllFileInspector();
                var shimArtifactSource = new FileSystemShimArtifactSource();
                var loadedAssemblySource = new AppDomainLoadedAssemblySource();

                var fileSystemScanner = new GameFileSystemScanner();
                var jsonParser = new UnityJsonParser();
                var knownIssuesDatabase = new JsonKnownIssuesDatabase(fileSystemScanner, jsonParser);

                var factory = new CollectorFactory(detector, harmony1Source, harmony2Source, assemblySource, pluginInfoSource, classifier, basicDllInspector, cecilDllInspector, shimArtifactSource, loadedAssemblySource, fileSystemScanner, knownIssuesDatabase);
                _reportBuilder = new DiagnosticReportBuilder(factory, detector, classifier);
                _overlay = new DiagnosticOverlay();
                _inputHandler = new InputHandler(_config);
                var externalLogCollector = factory.CreateExternalLogCollector();
                _logFileWriter = new LogFileWriter(fileManager, new ReportFormatter(), externalLogCollector);

                DiagnosticDataRegistry.Register(new DiagnosticDataProvider(_reportBuilder, externalLogCollector));

                _startTime = Time.time;
                _report = _reportBuilder.BuildReport();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }

        private void OnDestroy()
        {
            DiagnosticDataRegistry.Unregister();
        }

        private void Update()
        {
            try
            {
                if (_inputHandler == null)
                {
                    return;
                }

                if (_inputHandler.ShouldToggleOverlay())
                {
                    _isOverlayVisible = !_isOverlayVisible;
                }

                if (_inputHandler.ShouldRefresh())
                {
                    RefreshReport();
                }

                if (_isOverlayVisible && _inputHandler.ShouldAutoRefresh(Time.time - _startTime, ++_frameCount))
                {
                    RefreshReport();
                }
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }

        private void OnGUI()
        {
            try
            {
                if (!_isOverlayVisible || _report == null || _overlay == null || _config == null)
                {
                    return;
                }

                _overlay.Draw(_report, _config, RefreshReport, GenerateLog);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }

        private void RefreshReport()
        {
            if (_reportBuilder == null)
            {
                return;
            }

            try
            {
                _report = _reportBuilder.BuildReport();
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }

        private void GenerateLog()
        {
            if (_logFileWriter == null || _report == null)
            {
                return;
            }

            try
            {
                var filePath = _logFileWriter.WriteReport(_report);
                _ = Process.Start("notepad.exe", filePath);
            }
            catch (Exception ex)
            {
                Harmony_Patch.Instance.Logger.WriteException(ex);
            }
        }
    }
}
