// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace LobotomyPlaywright.Protocol
{
    public class Request
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Params { get; set; }
        public List<string> Events { get; set; }
    }
}
