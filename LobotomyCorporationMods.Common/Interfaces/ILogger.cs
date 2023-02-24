// SPDX-License-Identifier: MIT

#region

using System;

#endregion

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface ILogger
    {
        bool DebugLoggingEnabled { get; set; }
        void WriteToLog(Exception exception);
    }
}
