// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;

#endregion

namespace Hemocode.Playwright.JsonModels
{
    /// <summary>
    /// Represents an inbound request message.
    /// Use lowercase field names for JSON compatibility with both JsonUtility and Newtonsoft.Json.
    /// </summary>
    [Serializable]
    public sealed class Request
    {
        public string id;
        public string type;
        public string target;
        public string action;
        public Dictionary<string, object> @params;
        public List<string> events;

        // PascalCase accessors for C# code
        public string Id { get => id; set => id = value; }
        public string Type { get => type; set => type = value; }
        public string Target { get => target; set => target = value; }
        public string Action { get => action; set => action = value; }
        public Dictionary<string, object> Params { get => @params; set => @params = value; }
        public List<string> Events { get => events; set => events = value; }
    }
}
