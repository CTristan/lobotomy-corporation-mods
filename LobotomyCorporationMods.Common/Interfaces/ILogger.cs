// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface ILogger
    {
        void AddTarget(ILoggerTarget target);
        void LogError(Exception exception);

        // ReSharper disable once UnusedMember.Global
        void LogDebug(string message);
    }
}
