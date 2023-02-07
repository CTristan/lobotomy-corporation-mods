// SPDX-License-Identifier: MIT

using System;

namespace LobotomyCorporationMods.Common.Interfaces
{
    public interface ILogger
    {
        void WriteToLog(Exception exception);
    }
}
