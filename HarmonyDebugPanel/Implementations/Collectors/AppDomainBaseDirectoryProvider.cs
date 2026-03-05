// SPDX-License-Identifier: MIT

using System;
using HarmonyDebugPanel.Interfaces;

namespace HarmonyDebugPanel.Implementations.Collectors
{
    public sealed class AppDomainBaseDirectoryProvider : IBaseDirectoryProvider
    {
        public string GetBaseDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
