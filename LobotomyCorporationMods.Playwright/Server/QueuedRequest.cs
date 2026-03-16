// SPDX-License-Identifier: MIT

#region

using System.Diagnostics.CodeAnalysis;
using LobotomyCorporationMods.Playwright.JsonModels;

#endregion

namespace LobotomyCorporationMods.Playwright.Server
{
    [ExcludeFromCodeCoverage]
    public sealed class QueuedRequest
    {
        public Request Request { get; private set; }
        public System.Net.Sockets.TcpClient Client { get; private set; }
        public System.Threading.Thread Thread { get; private set; }

        public QueuedRequest(Request request, System.Net.Sockets.TcpClient client, System.Threading.Thread thread)
        {
            Request = request;
            Client = client;
            Thread = thread;
        }
    }
}
