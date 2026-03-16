// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

#endregion

namespace LobotomyCorporationMods.Test.Attributes
{
    /// <summary>
    ///     Enforces a global timeout on all tests. When applied at the assembly level,
    ///     any test that takes longer than the specified duration will fail with a
    ///     descriptive timeout message.
    /// </summary>
    public sealed class GlobalTimeoutAttribute : BeforeAfterTestAttribute
    {
        private Stopwatch? _stopwatch;

        public GlobalTimeoutAttribute(int timeoutMilliseconds)
        {
            if (timeoutMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeoutMilliseconds), "Timeout must be a positive number of milliseconds.");
            }

            TimeoutMilliseconds = timeoutMilliseconds;
        }

        public int TimeoutMilliseconds { get; }

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            _stopwatch?.Stop();

            if (_stopwatch is not null && _stopwatch.ElapsedMilliseconds > TimeoutMilliseconds)
            {
                throw TestTimeoutException.ForTimedOutTest(TimeoutMilliseconds);
            }
        }
    }
}
