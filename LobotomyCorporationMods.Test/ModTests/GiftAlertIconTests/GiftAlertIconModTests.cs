// SPDX-License-Identifier: MIT

using System;
using LobotomyCorporationMods.GiftAlertIcon;
using LobotomyCorporationMods.Test.Extensions;

namespace LobotomyCorporationMods.Test.ModTests.GiftAlertIconTests
{
    public class GiftAlertIconModTests : IDisposable
    {
        protected GiftAlertIconModTests()
        {
            _ = new Harmony_Patch();
            var mockLogger = TestExtensions.GetMockLogger();
            Harmony_Patch.Instance.AddLoggerTarget(mockLogger.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnityTestExtensions.ResetStaticFields();
            }
        }
    }
}
