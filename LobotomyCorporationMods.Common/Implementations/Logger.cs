// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
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

        [NotNull]
        private static string Timestamp => DateTimeOffset.UtcNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

        public void AddTarget(ILoggerTarget target)
        {
            _targets.Add(target);
        }

        public void LogException(Exception exception)
        {
            var message = $"{Timestamp} ERROR: {exception}";

            foreach (var target in _targets)
            {
                target.WriteToLoggerTarget(message);
            }
        }

        public void LogInfo(string message)
        {
            message = $"{Timestamp} INFO: {message}";

            var target = _targets[0];

            target.WriteToLoggerTarget(message);
        }

        public void LogWarning(string message)
        {
            message = $"{Timestamp} WARNING: {message}";

            var target = _targets[0];

            target.WriteToLoggerTarget(message);
        }
    }
}
