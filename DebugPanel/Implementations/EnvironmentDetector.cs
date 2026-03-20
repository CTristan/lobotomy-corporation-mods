// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using DebugPanel.Common.Attributes;
using DebugPanel.Common.Constants;
using DebugPanel.Interfaces;
using DebugPanel.Common.Models.Diagnostics;

#endregion

namespace DebugPanel.Implementations
{
    [AdapterClass]
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    public sealed class EnvironmentDetector : IEnvironmentDetector
    {
        private bool _detected;
        private bool _isHarmony2Available;
        private bool _isBepInExAvailable;
        private bool _isMonoCecilAvailable;

        public bool IsHarmony2Available
        {
            get
            {
                EnsureDetected();

                return _isHarmony2Available;
            }
        }

        public bool IsBepInExAvailable
        {
            get
            {
                EnsureDetected();

                return _isBepInExAvailable;
            }
        }

        public bool IsMonoCecilAvailable
        {
            get
            {
                EnsureDetected();

                return _isMonoCecilAvailable;
            }
        }

        public EnvironmentInfo DetectEnvironment()
        {
            _detected = false;
            EnsureDetected();

            return new EnvironmentInfo(_isHarmony2Available, _isBepInExAvailable, _isMonoCecilAvailable);
        }

        private void EnsureDetected()
        {
            if (_detected)
            {
                return;
            }

            _isHarmony2Available = false;
            _isBepInExAvailable = false;
            _isMonoCecilAvailable = false;

            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly == null)
                    {
                        continue;
                    }

                    try
                    {
                        if (!_isHarmony2Available)
                        {
                            var harmonyType = assembly.GetType("HarmonyLib.Harmony");
                            if (harmonyType != null)
                            {
                                _isHarmony2Available = true;
                            }
                        }

                        if (!_isBepInExAvailable)
                        {
                            var chainloaderType = assembly.GetType("BepInEx.Bootstrap.Chainloader");
                            if (chainloaderType != null)
                            {
                                _isBepInExAvailable = true;
                            }
                        }

                        if (!_isMonoCecilAvailable)
                        {
                            var cecilType = assembly.GetType("Mono.Cecil.AssemblyDefinition");
                            if (cecilType != null)
                            {
                                _isMonoCecilAvailable = true;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore individual assembly inspection failures
                    }
                }
            }
            catch (Exception)
            {
                // If AppDomain scanning fails entirely, all flags remain false
            }

            _detected = true;
        }
    }
}
