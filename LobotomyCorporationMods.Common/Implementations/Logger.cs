// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using LobotomyCorporationMods.Common.Enums;
using LobotomyCorporationMods.Common.Interfaces;

#endregion

namespace LobotomyCorporationMods.Common.Implementations
{
    public sealed class Logger : ILogger
    {
        private readonly List<ILoggerTarget> _targets = new List<ILoggerTarget>();

        public Logger(ILoggerTarget loggerTarget)
        {
            _targets.Add(loggerTarget);
        }

        public LogLevels MinLevel { get; set; } = LogLevels.Warning;

        public void AddTarget(ILoggerTarget target)
        {
            _targets.Add(target);
        }

        public IEnumerable<ILoggerTarget> GetTargets()
        {
            return _targets;
        }

        public void LogException(Exception exception)
        {
            if (exception == null)
            {
                return;
            }

            Log(exception.ToString(), LogLevels.Error);
        }

        public void Log(string message, LogLevels loglevel = LogLevels.Info)
        {
            if (loglevel < MinLevel)
            {
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            foreach (var target in _targets)
            {
                target.WriteToLoggerTarget($"[{timestamp}] {loglevel.ToString().ToUpperInvariant()}: {message}");
            }
        }
    }
}
