// SPDX-License-Identifier: MIT

#region

using System;
using JetBrains.Annotations;

#endregion

namespace LobotomyCorporationMods.Common.Attributes
{
    /// <inheritdoc />
    /// <summary>
    ///     Should only be used for adapter classes that will always throw a Unity exception when called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property |
                    AttributeTargets.Event,
        Inherited = false)]
    internal sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        internal ExcludeFromCodeCoverageAttribute([NotNull] string justification)
        {
            if (string.IsNullOrEmpty(justification))
            {
                throw new InvalidOperationException("Code excluded from coverage must have a valid reason.");
            }
        }
    }
}
