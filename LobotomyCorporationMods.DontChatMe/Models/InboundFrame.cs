// SPDX-License-Identifier: MIT

#region

using LobotomyCorporationMods.DontChatMe.Constants;

#endregion

namespace LobotomyCorporationMods.DontChatMe.Models
{
    /// <summary>
    ///     A parsed view of any inbound frame.
    ///     The transport peeks at <see cref="Type" /> to decide what to do, then asks
    ///     <see cref="TryGetEffectDispatch" /> when it needs the typed payload.
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
        ///     Tries to parse <paramref name="json" /> into an inbound frame.
        ///     Returns <c>false</c> if the input is not a JSON object or has no <c>type</c> field.
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
        ///     If this frame is an <c>effect_dispatched</c>, hydrates and returns the typed payload.
        ///     Returns <c>false</c> otherwise (wrong type, missing required fields).
        /// </summary>
        public bool TryGetEffectDispatch(out EffectDispatch dispatch)
        {
            if (Type != WireTypes.EffectDispatched)
            {
                dispatch = null;
                return false;
            }

            return EffectDispatch.TryFromJson(_json, out dispatch);
        }

        /// <summary>Reads the <c>error.code</c> on an inbound <c>error</c> frame (or <c>null</c>).</summary>
        public string GetErrorCode() => _json.GetString(JsonKeys.Code);

        /// <summary>Reads the <c>error.message</c> on an inbound <c>error</c> frame (or <c>null</c>).</summary>
        public string GetErrorMessage() => _json.GetString(JsonKeys.Message);
    }
}
