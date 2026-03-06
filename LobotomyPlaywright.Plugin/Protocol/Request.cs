// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LobotomyPlaywright.Protocol
{
    /// <summary>
    /// Represents an inbound request message.
    /// Use lowercase field names for JSON compatibility with both JsonUtility and Newtonsoft.Json.
    /// </summary>
    [SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
    [Serializable]
    public class Request
    {
        public string id;
        public string type;
        public string target;
        public string action;
        public Dictionary<string, object> @params;
        public List<string> events;

        // PascalCase accessors for C# code
        public string Id { get { return id; } set { id = value; } }
        public string Type { get { return type; } set { type = value; } }
        public string Target { get { return target; } set { target = value; } }
        public string Action { get { return action; } set { action = value; } }
        public Dictionary<string, object> Params { get { return @params; } set { @params = value; } }
        public List<string> Events { get { return events; } set { events = value; } }
    }
}
