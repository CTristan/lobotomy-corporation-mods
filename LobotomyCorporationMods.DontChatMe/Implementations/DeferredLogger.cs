// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using LobotomyCorporation.Mods.Common;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Implementations
{
    /// <summary>
    ///     ILogger adapter that resolves the actual logger lazily on every call.
    ///     The Harmony_Patch singleton constructs its dispatcher before <see cref="Logger" /> is set
    ///     by the host (or by tests via <c>SetLogger</c>); this indirection prevents a null-deref.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = Messages.UnityCodeCoverageJustification)]
    internal sealed class DeferredLogger : ILogger
    {
        private readonly Func<ILogger> _provider;

        public DeferredLogger(Func<ILogger> provider)
        {
            ThrowHelper.ThrowIfNull(provider, nameof(provider));
            _provider = provider;
        }

        public void WriteException(Exception exception)
        {
            _provider()?.WriteException(exception);
        }

        public void AddTarget(ILoggerTarget target)
        {
            _provider()?.AddTarget(target);
        }
    }
}
