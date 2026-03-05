// SPDX-License-Identifier: MIT

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

#pragma warning disable CA1810 // Static constructor needed for runtime Unity detection

namespace LobotomyPlaywright.Protocol
{
    /// <summary>
    /// JSON serializer compatible with .NET 3.5 and Unity.
    /// Uses JsonUtility when running in Unity, falls back to Newtonsoft.Json otherwise.
    /// </summary>
    public static class MessageSerializer
    {
        static MessageSerializer()
        {
            try
            {
                // Try to use JsonUtility - if it throws SecurityException, fall back to Newtonsoft.Json
                JsonUtility.ToJson(new { test = true });
                s_useNewtonsoftJson = false;
            }
            catch (System.Security.SecurityException)
            {
                // Unity's internal calls don't work outside Unity runtime
                s_useNewtonsoftJson = true;
                InitializeNewtonsoftJson();
            }
            catch
            {
                // Other exceptions, assume JsonUtility works
                s_useNewtonsoftJson = false;
            }
        }

        private static bool s_useNewtonsoftJson;
        private static MethodInfo s_newtonsoftSerializeObject;
        private static MethodInfo s_newtonsoftDeserializeObject;

        private static void InitializeNewtonsoftJson()
        {
            try
            {
                var newtonsoftAssembly = Assembly.Load("Newtonsoft.Json");
                if (newtonsoftAssembly != null)
                {
                    var jsonConvertType = newtonsoftAssembly.GetType("Newtonsoft.Json.JsonConvert");
                    if (jsonConvertType != null)
                    {
                        s_newtonsoftSerializeObject = jsonConvertType.GetMethod("SerializeObject", new[] { typeof(object) });
                        s_newtonsoftDeserializeObject = jsonConvertType.GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
                    }
                }
            }
            catch
            {
                // Newtonsoft.Json not available, will throw at runtime
            }
        }

        public static string Serialize(Response response)
        {
            if (s_useNewtonsoftJson)
            {
                if (s_newtonsoftSerializeObject != null)
                {
                    return (string)s_newtonsoftSerializeObject.Invoke(null, new object[] { response });
                }
                throw new InvalidOperationException("Newtonsoft.Json is not available");
            }
            return JsonUtility.ToJson(response);
        }

        public static Request DeserializeRequest(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException(nameof(json));
            }

            try
            {
                if (s_useNewtonsoftJson)
                {
                    if (s_newtonsoftDeserializeObject != null)
                    {
                        return (Request)s_newtonsoftDeserializeObject.Invoke(null, new object[] { json, typeof(Request) });
                    }
                    throw new InvalidOperationException("Newtonsoft.Json is not available");
                }
                return JsonUtility.FromJson<Request>(json);
            }
            catch (Exception ex) when (!(ex is System.Security.SecurityException))
            {
                throw new InvalidOperationException($"Failed to deserialize request: {ex.Message}", ex);
            }
        }
    }
}

#pragma warning restore CA1810
