// SPDX-License-Identifier: MIT

#region

using System.Collections.Generic;
using System.Globalization;
using LobotomyCorporation.Mods.Common;
using SharpJson;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     Typed accessor over the <see cref="Dictionary{TKey,TValue}" /> that
    ///     <see cref="JsonDecoder.DecodeText(string)" /> hands back.
    ///     Lookups return defaults rather than throwing so a malformed inbound frame
    ///     becomes a structured rejection, not an exception in the read loop.
    /// </summary>
    internal sealed class JsonDict
    {
        private readonly IDictionary<string, object> _raw;

        internal JsonDict(IDictionary<string, object> raw)
        {
            ThrowHelper.ThrowIfNull(raw, nameof(raw));
            _raw = raw;
        }

        /// <summary>Tries to parse a top-level JSON object out of <paramref name="json" />.</summary>
        /// <returns><c>true</c> when the input is a JSON object; <c>false</c> otherwise.</returns>
        internal static bool TryParse(string json, out JsonDict result)
        {
            result = null;
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            object decoded;
            try
            {
                decoded = JsonDecoder.DecodeText(json);
            }
#pragma warning disable CA1031 // SharpJson throws a wide range of exception types on malformed input; we treat all of them as a structured parse failure rather than letting one tear down the receive loop.
            catch (System.Exception)
#pragma warning restore CA1031
            {
                return false;
            }

            var asDict = decoded as IDictionary<string, object>;
            if (asDict == null)
            {
                return false;
            }

            result = new JsonDict(asDict);
            return true;
        }

        internal bool ContainsKey(string key) => _raw.ContainsKey(key);

        internal string GetString(string key)
        {
            object value;
            if (!_raw.TryGetValue(key, out value) || value == null)
            {
                return null;
            }

            return value as string ?? value.ToString();
        }

        internal int GetInt(string key, int defaultValue)
        {
            object value;
            if (!_raw.TryGetValue(key, out value) || value == null)
            {
                return defaultValue;
            }

            try
            {
                return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }
#pragma warning disable CA1031 // Convert.ToInt32 throws several distinct exception types for bad input; the right answer in every case is to fall back to the caller-supplied default rather than crash the parse loop.
            catch (System.Exception)
#pragma warning restore CA1031
            {
                return defaultValue;
            }
        }
    }
}
