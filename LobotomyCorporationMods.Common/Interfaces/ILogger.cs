// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface ILogger
    {
        void AddTarget(ILoggerTarget target);
        IEnumerable<ILoggerTarget> GetTargets();
        void WriteException(Exception exception);
        void WriteInfo(string message);
    }
}
