// SPDX-License-Identifier: MIT

#region

using System;
using AwesomeAssertions;
using LobotomyCorporationMods.Test.Attributes;
using Xunit;
using Xunit.Sdk;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.CommonTests
{
    public sealed class GlobalTimeoutAttributeTests
    {
        [Fact]
        public void Constructor_throws_when_timeout_is_zero()
        {
            var act = () => new GlobalTimeoutAttribute(0);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Constructor_throws_when_timeout_is_negative()
        {
            var act = () => new GlobalTimeoutAttribute(-1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void After_does_not_throw_when_test_completes_within_timeout()
        {
            var attribute = new GlobalTimeoutAttribute(10000);
            var method = typeof(GlobalTimeoutAttributeTests).GetMethod(nameof(After_does_not_throw_when_test_completes_within_timeout))!;

            attribute.Before(method, null!);
            attribute.After(method, null!);
        }

        [Fact]
        public void After_throws_TestTimeoutException_when_elapsed_exceeds_timeout()
        {
            var attribute = new GlobalTimeoutAttribute(1);
            var method = typeof(GlobalTimeoutAttributeTests).GetMethod(nameof(After_throws_TestTimeoutException_when_elapsed_exceeds_timeout))!;

            attribute.Before(method, null!);
            System.Threading.Thread.Sleep(50);

            var act = () => attribute.After(method, null!);

            act.Should().Throw<TestTimeoutException>();
        }

        [Fact]
        public void After_does_not_throw_when_Before_was_not_called()
        {
            var attribute = new GlobalTimeoutAttribute(10000);
            var method = typeof(GlobalTimeoutAttributeTests).GetMethod(nameof(After_does_not_throw_when_Before_was_not_called))!;

            attribute.After(method, null!);
        }
    }
}
