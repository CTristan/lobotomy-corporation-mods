// SPDX-License-Identifier: MIT

#region

using System;
using LobotomyCorporation.Mods.Common;
using LobotomyCorporationMods.DontChatMe;
using LobotomyCorporationMods.Test.Extensions;
using Moq;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests
{
    public class DontChatMeModTests : IDisposable
    {
        protected DontChatMeModTests()
        {
            _ = new Harmony_Patch();
            var logger = new Mock<ILogger>();
            Harmony_Patch.Instance.SetLogger(logger.Object);
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
