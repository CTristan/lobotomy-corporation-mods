// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Reflection;

namespace LobotomyPlaywright.Plugin.Test.Tests
{
    /// <summary>
    /// Helper to check if Unity runtime is available for testing.
    /// </summary>
    public static class UnityTestHelper
    {
        private static bool? s_isUnityAvailable;

        public static bool IsUnityAvailable
        {
            get
            {
                if (!s_isUnityAvailable.HasValue)
                {
                    try
                    {
                        // Try to access a Unity type to check if Unity runtime is available
                        Type unityObjectType = Type.GetType("UnityEngine.Object, UnityEngine.CoreModule");
                        if (unityObjectType != null)
                        {
                            // Try to use JsonUtility which is an internal call
                            Type jsonUtilityType = Type.GetType("UnityEngine.JsonUtility, UnityEngine.CoreModule");
                            if (jsonUtilityType != null)
                            {
                                var toJsonMethod = jsonUtilityType.GetMethod("ToJson", [typeof(object)]);
                                if (toJsonMethod != null)
                                {
                                    // Try to call it - if it works, Unity runtime is available
                                    try
                                    {
                                        _ = toJsonMethod.Invoke(null, [new { test = true }]);
                                        s_isUnityAvailable = true;
                                        return true;
                                    }
                                    catch (TargetInvocationException ex) when (ex.InnerException is System.Security.SecurityException)
                                    {
                                        // SecurityException means Unity DLLs are loaded but internal calls don't work
                                        // (e.g., running in .NET test environment, not Unity runtime)
                                        s_isUnityAvailable = false;
                                        return false;
                                    }
                                    catch (TargetInvocationException)
                                    {
                                        // Other invocation exceptions mean Unity is not available
                                        s_isUnityAvailable = false;
                                        return false;
                                    }
                                    catch (System.Security.SecurityException)
                                    {
                                        // Direct SecurityException means Unity is not available
                                        s_isUnityAvailable = false;
                                        return false;
                                    }
                                }
                            }
                        }
                        s_isUnityAvailable = false;
                    }
                    catch (FileNotFoundException)
                    {
                        // Unity DLLs not found
                        s_isUnityAvailable = false;
                    }
                    catch (TargetInvocationException)
                    {
                        // Invocation failed, Unity not available
                        s_isUnityAvailable = false;
                    }
                    catch (System.Security.SecurityException)
                    {
                        // Security exception, Unity not available
                        s_isUnityAvailable = false;
                    }
                }
                return s_isUnityAvailable.Value;
            }
        }
    }
}
