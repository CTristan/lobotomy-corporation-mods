// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LobotomyCorporationMods.DontChatMe.Transport;

#endregion

namespace LobotomyCorporationMods.Test.ModTests.DontChatMeTests.Fakes
{
    /// <summary>
    ///     Hand-rolled fake for <see cref="IWebSocket" />.
    ///     Tests drive the lifecycle by calling <see cref="SimulateOpen" />, <see cref="SimulateMessage" />, etc.
    /// </summary>
    public sealed class FakeWebSocket : IWebSocket
    {
        private readonly List<string> _sent = new List<string>();

        public Uri ConstructedFor { get; set; }
        public bool ConnectCalled { get; private set; }
        public bool CloseCalled { get; private set; }
        public bool Disposed { get; private set; }
        public bool IsAlive { get; set; } = true;

        public Collection<string> Sent => new Collection<string>(_sent);

        public event Action Opened;
        public event Action<string> MessageReceived;
        public event Action Closed;
        public event Action<Exception> ErrorOccurred;

        public void Connect()
        {
            ConnectCalled = true;
        }

        public void Send(string text)
        {
            _sent.Add(text);
        }

        public void Close()
        {
            CloseCalled = true;
            IsAlive = false;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        // Test driver helpers

        public void SimulateOpen() => Opened?.Invoke();

        public void SimulateMessage(string text) => MessageReceived?.Invoke(text);

        public void SimulateClose()
        {
            IsAlive = false;
            Closed?.Invoke();
        }

        public void SimulateError(Exception ex) => ErrorOccurred?.Invoke(ex);
    }
}
