// SPDX-License-Identifier: MIT

#region

using System;
using System.Diagnostics.CodeAnalysis;
using Hemocode.Playwright.JsonModels;
using UnityEngine;

#endregion

namespace Hemocode.Playwright.Protocol
{
    /// <summary>
    /// JSON serializer compatible with .NET 3.5 and Unity.
    /// Uses JsonUtility exclusively with "coerced" placeholder trick for object fields.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MessageSerializer
    {
        private const string DataPlaceholder = "__DATA_PLACEHOLDER__";

        public static string Serialize(Response response)
        {
            if (response == null)
            {
                return "null";
            }

            // If we have no data object, just serialize as is
            if (response.DataObject == null)
            {
                response.data = null;
                return JsonUtility.ToJson(response);
            }

            // Set placeholder and serialize wrapper
            response.data = DataPlaceholder;
            var wrapperJson = JsonUtility.ToJson(response);

            // Serialize data object
            var dataJson = JsonUtility.ToJson(response.DataObject);

            // Replace placeholder with data JSON (it might be empty or "{}")
            return wrapperJson.Replace("\"" + DataPlaceholder + "\"", dataJson);
        }

        public static Request DeserializeRequest(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            try
            {
                return JsonUtility.FromJson<Request>(json);
            }
            catch (Exception ex) when (!(ex is System.Security.SecurityException))
            {
                throw new InvalidOperationException($"Failed to deserialize request: {json}", ex);
            }
        }
    }
}
