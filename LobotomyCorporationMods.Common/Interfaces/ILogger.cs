// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using LobotomyCorporationMods.Common.Enums;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface ILogger
    {
        LogLevels MinLevel { get; set; }
        void AddTarget(ILoggerTarget target);
        IEnumerable<ILoggerTarget> GetTargets();
        void LogException(Exception exception);
        void Log(string message, LogLevels loglevel = LogLevels.Info);
    }
}
