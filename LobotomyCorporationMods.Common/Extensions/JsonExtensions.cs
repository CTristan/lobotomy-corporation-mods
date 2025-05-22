// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using System.Text;
using LobotomyCorporationMods.Common.Implementations;
using SharpJson;

namespace LobotomyCorporationMods.Common.Extensions
{
    public static class JsonExtensions
    {
        public static Dictionary<string, object> DecodeText(this string json)
        {
            return (Dictionary<string, object>)JsonDecoder.DecodeText(json);
        }

        public static string BuildJson(Dictionary<string, object> fields)
        {
            Guard.Against.Null(fields, nameof(fields));

            var sb = new StringBuilder();
            sb.Append('{');

            var first = true;
            foreach (var kvp in fields.Where(kvp => kvp.Value != null))
            {
                if (!first)
                {
                    sb.Append(',');
                }

                sb.Append('\"').Append(kvp.Key).Append("\":");

                switch (kvp.Value)
                {
                    case string value:
                        sb.Append('\"').Append(JsonEscape(value)).Append('\"');
                        break;
                    case bool value:
                        sb.Append(value ? "true" : "false");
                        break;
                    default:
                        sb.Append(kvp.Value); // assumes number or already stringified
                        break;
                }

                first = false;
            }

            sb.Append('}');
            return sb.ToString();
        }

        public static string JsonEscape(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "";
            }

            return input
                .Replace("\\", @"\\")
                .Replace("\"", "\\\"")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        public static string TryGetString(this Dictionary<string, object> dict, string key)
        {
            Guard.Against.Null(dict, nameof(dict));

            if (dict.TryGetValue(key, out var value) && value is string str)
            {
                return str;
            }

            return null;
        }

        public static int? TryGetNullableInt(this Dictionary<string, object> dict, string key)
        {
            Guard.Against.Null(dict, nameof(dict));

            if (!dict.TryGetValue(key, out var value) || value == null)
            {
                return null;
            }

            switch (value)
            {
                case int i:
                    return i;
                // Handle boxed long → int? or string → int?
                case long l:
                    return (int)l;
            }

            if (int.TryParse(value.ToString(), out var result))
            {
                return result;
            }

            return null;
        }
    }
}
