// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     A parsed view of any inbound frame. The transport peeks at <see cref="Type" /> to
    ///     route, then calls the typed accessor for fields it needs.
    /// </summary>
    public sealed class InboundFrame
    {
        private readonly JsonDict _json;

        private InboundFrame(string type, JsonDict json)
        {
            Type = type;
            _json = json;
        }

        public string Type { get; }

        /// <summary>
        ///     Tries to parse <paramref name="json" /> into an inbound frame. Returns <c>false</c>
        ///     when the input is not a JSON object or has no <c>type</c> field.
        /// </summary>
        public static bool TryParse(string json, out InboundFrame frame)
        {
            frame = null;
            JsonDict dict;
            if (!JsonDict.TryParse(json, out dict))
            {
                return false;
            }

            var type = dict.GetString(JsonKeys.Type);
            if (string.IsNullOrEmpty(type))
            {
                return false;
            }

            frame = new InboundFrame(type, dict);
            return true;
        }

        /// <summary>
        ///     If this frame is an <c>effect_dispatch</c>, hydrates and returns the typed payload.
        ///     Returns <c>false</c> otherwise (wrong type or missing required fields).
        /// </summary>
        public bool TryGetEffectDispatch(out EffectDispatch dispatch)
        {
            if (Type != WireTypes.EffectDispatch)
            {
                dispatch = null;
                return false;
            }

            return EffectDispatch.TryFromJson(_json, out dispatch);
        }

        /// <summary>Monotonic frame identifier set by the server; <c>0</c> when absent.</summary>
        public int GetId() => _json.GetInt(JsonKeys.Id, defaultValue: 0);

        /// <summary>Reads the <c>error.code</c> on an inbound <c>error</c> frame (or <c>null</c>).</summary>
        public string GetErrorCode() => _json.GetString(JsonKeys.Code);

        /// <summary>Reads the <c>error.message</c> on an inbound <c>error</c> frame (or <c>null</c>).</summary>
        public string GetErrorMessage() => _json.GetString(JsonKeys.Message);

        /// <summary>
        ///     Welcome metadata: queue depth at hello time. <c>0</c> when absent. The HUD shows
        ///     this so the streamer knows how many redemptions are waiting on reconnect.
        /// </summary>
        public int GetWelcomeQueueDepth() => _json.GetInt(JsonKeys.QueueDepth, defaultValue: 0);

        /// <summary>
        ///     Welcome metadata: the in-flight redemption_id at hello time (the one the server
        ///     is about to resend with <c>replay: true</c>), or <c>null</c> when the queue's
        ///     head is empty.
        /// </summary>
        public string GetWelcomeInFlightRedemptionId() =>
            _json.GetString(JsonKeys.InFlightRedemptionId);
    }
}
