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
        private static string Timestamp => DateTimeOffset.Now.ToString("u", CultureInfo.InvariantCulture);

        public void AddTarget(ILoggerTarget target)
        {
            _targets.Add(target);
        }

        public void LogError(Exception exception)
        {
            var message = $"{Timestamp} ERROR: {exception}";

            foreach (var target in _targets)
            {
                target.WriteToLoggerTarget(message + Environment.NewLine);
            }
        }

        public void LogDebug(string message)
        {
            message = $"{Timestamp} DEBUG: {message}";

            _targets[0].WriteToLoggerTarget(message + Environment.NewLine);
        }
    }
}
