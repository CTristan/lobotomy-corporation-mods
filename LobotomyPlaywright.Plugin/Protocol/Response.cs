// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;

namespace LobotomyPlaywright.Protocol
{
    /// <summary>
    /// Represents an outbound response message.
    /// Use lowercase field names for JSON compatibility.
    /// Note: data is a string field to work around JsonUtility limitations.
    /// It is used as a placeholder during serialization.
    /// </summary>
    [SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
    [Serializable]
    public class Response
    {
        public string id;
        public string type;
        public string status;
        public string data; // Changed to string for JsonUtility coercion
        public string error;
        public string code;
        public string @event;
        public string timestamp;

        // Non-serialized field to hold the actual data object
        [NonSerialized]
        public object DataObject;

        // PascalCase accessors for C# code
        public string Id { get { return id; } set { id = value; } }
        public string Type { get { return type; } set { type = value; } }
        public string Status { get { return status; } set { status = value; } }
        public string Data { get { return data; } set { data = value; } }
        public string Error { get { return error; } set { error = value; } }
        public string Code { get { return code; } set { code = value; } }
        public string Event { get { return @event; } set { @event = value; } }
        public string Timestamp { get { return timestamp; } set { timestamp = value; } }

        public static Response CreateSuccess(string requestId, object responseData)
        {
            return new Response
            {
                id = requestId,
                type = "response",
                status = "ok",
                DataObject = responseData
            };
        }

        public static Response CreateError(string requestId, string errorMessage, string errorCode = null)
        {
            return new Response
            {
                id = requestId,
                type = "response",
                status = "error",
                error = errorMessage,
                code = errorCode ?? "UNKNOWN_ERROR"
            };
        }

        public static Response CreateEvent(string eventName, object eventData)
        {
            return new Response
            {
                id = null,
                type = "event",
                @event = eventName,
                DataObject = eventData,
                timestamp = DateTime.UtcNow.ToString("O")
            };
        }
    }
}
