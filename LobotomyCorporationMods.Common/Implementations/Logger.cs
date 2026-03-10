// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
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

        public void AddTarget(ILoggerTarget target)
        {
            _targets.Add(target);
        }

        public void WriteException(Exception exception)
        {
            var message = $"ERROR: {exception}";

            foreach (var target in _targets)
            {
                target.WriteToLoggerTarget(message);
            }
        }
    }
}
