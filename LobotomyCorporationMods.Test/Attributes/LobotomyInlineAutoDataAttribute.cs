// SPDX-License-Identifier: MIT

#region

using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit3;
using LobotomyCorporationMods.Test.Customizations;

#endregion

namespace LobotomyCorporationMods.Test.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LobotomyInlineAutoDataAttribute(params object[] values)
        : InlineAutoDataAttribute(CreateFixture, values)
    {
        private static IFixture CreateFixture()
        {
            return new Fixture()
                .Customize(new AutoMoqCustomization())
                .Customize(new UnityCustomization());
        }
    }
}
