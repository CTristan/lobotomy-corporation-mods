// SPDX-License-Identifier: MIT

using LobotomyPlaywright.JsonModels;

namespace LobotomyPlaywright.Server
{
    public class QueuedRequest
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
