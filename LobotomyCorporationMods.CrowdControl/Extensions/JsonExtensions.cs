// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using SharpJson;

namespace LobotomyCorporationMods.CrowdControl.Extensions
{
    public static class JsonExtensions
    {
        public static Dictionary<string, object> DecodeText(this string json)
        {
            return (Dictionary<string, object>)JsonDecoder.DecodeText(json);
        }
    }
}
