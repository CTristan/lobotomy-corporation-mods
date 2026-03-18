// SPDX-License-Identifier: MIT

#region

using Hemocode.Common.Interfaces;

#endregion

namespace Hemocode.Common.Implementations
{
    public static class DiagnosticDataRegistry
    {
        public static IDiagnosticDataProvider Provider { get; private set; }

        public static bool IsRegistered => Provider != null;

        public static void Register(IDiagnosticDataProvider provider)
        {
            ThrowHelper.ThrowIfNull(provider);
            Provider = provider;
        }

        public static void Unregister()
        {
            Provider = null;
        }
    }
}
