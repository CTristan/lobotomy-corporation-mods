// SPDX-License-Identifier: MIT

using System;

namespace LobotomyCorporationMods.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
    internal sealed class GuardClauseAttribute : Attribute
    {
    }
}
