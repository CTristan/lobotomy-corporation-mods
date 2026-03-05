// SPDX-License-Identifier: MIT

using System;

namespace LobotomyPlaywright.Protocol
{
    public class Response
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public object Data { get; set; }
        public string Error { get; set; }
        public string Code { get; set; }
        public string Event { get; set; }
        public string Timestamp { get; set; }

        public static Response CreateSuccess(string id, object data)
        {
            return new Response
            {
                Id = id,
                Type = "response",
                Status = "ok",
                Data = data
            };
        }

        public static Response CreateError(string id, string error, string code = null)
        {
            return new Response
            {
                Id = id,
                Type = "response",
                Status = "error",
                Error = error,
                Code = code ?? "UNKNOWN_ERROR"
            };
        }

        public static Response CreateEvent(string eventName, object data)
        {
            return new Response
            {
                Id = null,
                Type = "event",
                Event = eventName,
                Data = data,
                Timestamp = DateTime.UtcNow.ToString("o")
            };
        }
    }
}
