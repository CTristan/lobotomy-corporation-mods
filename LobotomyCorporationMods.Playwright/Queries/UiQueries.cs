// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Queries for UI state and accessibility tree information.
    /// Provides structured text representation of the game's UI for text-only agents.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class UiQueries
    {
        private const int MaxNodesPerWindow = 50;
        private const int MaxPathDepth = 4;
        private const int MaxModElements = 20;
        private static int s_mainThreadId = -1;
        private static bool s_disableThreadCheck;

        /// <summary>
        /// Disables the Unity main thread check. For testing purposes only.
        /// WARNING: Disabling this check in production can lead to crashes and undefined behavior.
        /// </summary>
        public static void DisableThreadCheckForTesting()
        {
            s_disableThreadCheck = true;
        }

        /// <summary>
        /// Gets the current UI state with the specified depth mode.
        /// WARNING: This method must be called from the Unity main thread. Unity APIs
        /// are not thread-safe and calling them from a background thread will cause crashes.
        /// </summary>
        /// <param name="depth">"summary" (window states only), "full" (windows + children), or "window" (specific window only).</param>
        /// <param name="windowFilter">Name of specific window to query (only used when depth="window").</param>
        /// <returns>UiStateData containing the requested UI information.</returns>
        public static UiStateData GetUiState(string depth = "full", string windowFilter = null)
        {
            // Track main thread ID on first call (assumes first call is from main thread)
            if (s_mainThreadId == -1)
            {
                s_mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }

            // Check if called from Unity main thread (unless disabled for testing)
            if (!s_disableThreadCheck)
            {
                var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if (currentThreadId != s_mainThreadId)
                {
                    // Throw a clear error - Unity APIs must be called from main thread
                    throw new InvalidOperationException(
                        "GetUiState must be called from the Unity main thread. " +
                        "Current thread ID: " + currentThreadId + ", Main thread ID: " + s_mainThreadId + ". " +
                        "Unity APIs are not thread-safe and will crash if called from background threads."
                    );
                }
            }

            var depthMode = ParseDepthMode(depth);
            var windows = new List<UiWindowData>();
            var knownWindows = CheckKnownWindows();

            foreach (var windowInfo in knownWindows)
            {
                // Skip if filtering and this window doesn't match
                if (depthMode == DepthMode.Window &&
                    !string.IsNullOrEmpty(windowFilter) &&
                    !string.Equals(windowInfo.Name, windowFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var windowData = new UiWindowData
                {
                    Name = windowInfo.Name,
                    IsOpen = windowInfo.IsOpen,
                    WindowType = windowInfo.WindowType
                };

                // Extract children for full/window mode when the window is open
                if ((depthMode == DepthMode.Full || depthMode == DepthMode.Window) &&
                    windowInfo.IsOpen &&
                    windowInfo.GameObject != null)
                {
                    windowData.Children = ExtractChildren(windowInfo.GameObject, MaxNodesPerWindow);
                }

                windows.Add(windowData);
            }

            var activatedSlots = GetActivatedSlots();
            var modElements = ScanModElements();

            return new UiStateData
            {
                Windows = windows,
                ActivatedSlots = activatedSlots,
                ModElements = modElements
            };
        }

        private enum DepthMode
        {
            Summary,
            Full,
            Window
        }

        private static DepthMode ParseDepthMode(string depth)
        {
            if (string.IsNullOrEmpty(depth))
            {
                return DepthMode.Full;
            }

            switch (depth.ToLowerInvariant())
            {
                case "summary":
                    return DepthMode.Summary;
                case "window":
                    return DepthMode.Window;
                case "full":
                default:
                    return DepthMode.Full;
            }
        }

        /// <summary>
        /// Checks the status of known game windows and panels.
        /// Returns a list of window information with open/closed states.
        /// </summary>
        private static List<WindowInfo> CheckKnownWindows()
        {
            var windows = new List<WindowInfo>();

            // List of known windows to check - each with a name and a lambda to get the GameObject/IsOpen status
            var windowChecks = new[]
            {
                // 1. Agent Info Window
                new
                {
                    Name = "AgentInfoWindow",
                    WindowType = "AgentInfo",
                    Check = (Func<GameObject, bool>)((_) => CheckWindowOpen("AgentInfoWindow", "CurrentWindow"))
                },

                // 2. Command Window
                new
                {
                    Name = "CommandWindow",
                    WindowType = "Command",
                    Check = (Func<GameObject, bool>)((_) => CheckWindowOpen("CommandWindow", "CurrentWindow"))
                },

                // 3. Creature Info Window
                new
                {
                    Name = "CreatureInfoWindow",
                    WindowType = "CreatureInfo",
                    Check = (Func<GameObject, bool>)((_) => CheckWindowOpen("CreatureInfoWindow", "CurrentWindow"))
                },

                // 4. Manual UI
                new
                {
                    Name = "ManualUI",
                    WindowType = "Manual",
                    Check = (Func<GameObject, bool>)((go) => CheckSingletonProperty("ManualUI", "Instance", go, out var isOpen) && isOpen)
                },

                // 5. Option UI
                new
                {
                    Name = "OptionUI",
                    WindowType = "Option",
                    Check = (Func<GameObject, bool>)((go) => CheckSingletonProperty("OptionUI", "Instance", go, out var isOpen) && isOpen)
                },

                // 6. Deploy UI
                new
                {
                    Name = "DeployUI",
                    WindowType = "Deploy",
                    Check = (Func<GameObject, bool>)((go) => CheckSingletonProperty("DeployUI", "instance", go, out var isOpen) && isOpen)
                },

                // 7. Research Window (uses FindObjectOfType)
                new
                {
                    Name = "ResearchWindow",
                    WindowType = "Research",
                    Check = (Func<GameObject, bool>)((_) => FindObjectOfTypeByType("ResearchWindow") != null)
                },

                // 8. Global Bullet Window
                new
                {
                    Name = "GlobalBulletWindow",
                    WindowType = "Bullet",
                    Check = (Func<GameObject, bool>)((_) => CheckWindowOpen("GlobalBulletWindow", "CurrentWindow"))
                },

                // 9. Customizing Window
                new
                {
                    Name = "CustomizingWindow",
                    WindowType = "Customizing",
                    Check = (Func<GameObject, bool>)((_) => CheckWindowOpen("CustomizingWindow", "CurrentWindow"))
                },

                // 10. Escape UI
                new
                {
                    Name = "EscapeUI",
                    WindowType = "Escape",
                    Check = (Func<GameObject, bool>)((_) => FindObjectOfTypeByType("EscapeUI") != null)
                },

                // 11. Mission UI / Mission Popup UI
                new
                {
                    Name = "MissionUI",
                    WindowType = "Mission",
                    Check = (Func<GameObject, bool>)((_) =>
                        FindObjectOfTypeByType("MissionUI") != null ||
                        FindObjectOfTypeByType("MissionPopupUI") != null)
                },

                // 12. Agent Gift Window (InGameUI namespace)
                new
                {
                    Name = "AgentGiftWindow",
                    WindowType = "Gift",
                    Check = (Func<GameObject, bool>)((_) => FindObjectOfTypeByType("AgentGiftWindow") != null)
                }
            };

            foreach (var windowCheck in windowChecks)
            {
                try
                {
                    var gameObject = FindWindowGameObject(windowCheck.Name);
                    var isOpen = windowCheck.Check(gameObject);

                    windows.Add(new WindowInfo
                    {
                        Name = windowCheck.Name,
                        WindowType = windowCheck.WindowType,
                        GameObject = isOpen ? gameObject : null,
                        IsOpen = isOpen
                    });
                }
                catch
                {
                    // If checking a window fails, mark it as closed
                    windows.Add(new WindowInfo
                    {
                        Name = windowCheck.Name,
                        WindowType = windowCheck.WindowType,
                        GameObject = null,
                        IsOpen = false
                    });
                }
            }

            return windows;
        }

        /// <summary>
        /// Extracts child UI elements from a GameObject hierarchy.
        /// </summary>
        /// <param name="root">The root GameObject to search from.</param>
        /// <param name="maxNodes">Maximum number of nodes to extract.</param>
        /// <returns>List of UI node data for child elements.</returns>
        private static List<UiNodeData> ExtractChildren(GameObject root, int maxNodes)
        {
            if (root == null)
            {
                return new List<UiNodeData>();
            }

            var nodes = new List<UiNodeData>();

            try
            {
                // Get all text components
                var texts = root.GetComponentsInChildren<Text>();
                foreach (var text in texts)
                {
                    if (nodes.Count >= maxNodes)
                    {
                        break;
                    }

                    if (text != null && text.gameObject != root)
                    {
                        nodes.Add(CreateNodeFromText(text, root));
                    }
                }

                // Get all button components
                var buttons = root.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    if (nodes.Count >= maxNodes)
                    {
                        break;
                    }

                    if (button != null && button.gameObject != root)
                    {
                        nodes.Add(CreateNodeFromButton(button, root));
                    }
                }

                // Get all toggle components
                var toggles = root.GetComponentsInChildren<Toggle>();
                foreach (var toggle in toggles)
                {
                    if (nodes.Count >= maxNodes)
                    {
                        break;
                    }

                    if (toggle != null && toggle.gameObject != root)
                    {
                        nodes.Add(CreateNodeFromToggle(toggle, root));
                    }
                }

                // Get all slider components
                var sliders = root.GetComponentsInChildren<Slider>();
                foreach (var slider in sliders)
                {
                    if (nodes.Count >= maxNodes)
                    {
                        break;
                    }

                    if (slider != null && slider.gameObject != root)
                    {
                        nodes.Add(CreateNodeFromSlider(slider, root));
                    }
                }
            }
            catch
            {
                // If extraction fails, return empty list
            }

            return nodes;
        }

        /// <summary>
        /// Builds a slash-delimited path from a node to the root.
        /// Capped at MaxPathDepth levels.
        /// </summary>
        /// <param name="node">The node transform.</param>
        /// <param name="root">The root transform.</param>
        /// <returns>Slash-delimited path string.</returns>
        private static string BuildNodePath(Transform node, Transform root)
        {
            if (node == null || root == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var current = node;

            while (current != null && current != root && parts.Count < MaxPathDepth)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        /// <summary>
        /// Gets the activated slot names from UIActivateManager.
        /// </summary>
        /// <returns>List of activated slot names (0-4).</returns>
        private static List<string> GetActivatedSlots()
        {
            var slots = new List<string>();

            try
            {
                var uiActivateManager = FindObjectOfTypeByType("UIActivateManager");
                if (uiActivateManager != null)
                {
                    var activatedField = uiActivateManager.GetType().GetField(
                        "activated",
                        BindingFlags.Public | BindingFlags.Instance
                    );

                    if (activatedField != null)
                    {
                        if (activatedField.GetValue(uiActivateManager) is Array activatedArray)
                        {
                            for (var i = 0; i < Math.Min(activatedArray.Length, 5); i++)
                            {
                                var slot = activatedArray.GetValue(i);
                                if (slot != null)
                                {
                                    slots.Add(slot.ToString());
                                }
                                else
                                {
                                    slots.Add(null);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // If getting slots fails, return empty list
            }

            return slots;
        }

        /// <summary>
        /// Scans for known mod-specific UI elements.
        /// </summary>
        /// <returns>List of detected mod UI elements.</returns>
        private static List<UiNodeData> ScanModElements()
        {
            var modElements = new List<UiNodeData>();

            try
            {
                // Look for known mod patterns in the scene
                // Example: GiftAlertIcon from BadLuckProtectionForGifts mod
                var allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var go in allGameObjects)
                {
                    if (modElements.Count >= MaxModElements)
                    {
                        break;
                    }

                    if (go != null && IsModElement(go))
                    {
                        var text = go.GetComponent<Text>();
                        var value = text != null ? text.text : go.name;

                        modElements.Add(new UiNodeData
                        {
                            Path = go.name,
                            Type = GetUiNodeType(go),
                            Value = value,
                            Interactable = IsInteractable(go)
                        });
                    }
                }
            }
            catch
            {
                // If scanning fails, return empty list
            }

            return modElements;
        }

        #region Helper Methods

        private static GameObject FindWindowGameObject(string windowName)
        {
            try
            {
                // Try to find via FindObjectOfType for window classes
                var found = FindObjectOfTypeByType(windowName);
                if (found != null)
                {
                    return found;
                }

                // Try to find GameObject by name
                var gameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (var go in gameObjects)
                {
                    if (go != null && go.name.Contains(windowName))
                    {
                        return go;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static bool CheckWindowOpen(string typeName, string propertyName)
        {
            try
            {
                var windowObj = FindObjectOfTypeByType(typeName);
                if (windowObj == null)
                {
                    return false;
                }

                var property = windowObj.GetType().GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                );

                if (property != null)
                {
                    var value = property.GetValue(null, null);
                    return value != null;
                }

                var field = windowObj.GetType().GetField(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                );

                if (field != null)
                {
                    var value = field.GetValue(null);
                    return value != null;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckSingletonProperty(string typeName, string propertyName, GameObject gameObject, out bool isOpen)
        {
            isOpen = false;
            try
            {
                var type = FindType(typeName);
                if (type == null)
                {
                    return false;
                }

                var property = type.GetProperty(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                );

                if (property != null)
                {
                    var instance = property.GetValue(null, null);
                    isOpen = instance != null;
                    return true;
                }

                var field = type.GetField(
                    propertyName,
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy
                );

                if (field != null)
                {
                    var instance = field.GetValue(null);
                    isOpen = instance != null;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static GameObject FindObjectOfTypeByType(string typeName)
        {
            try
            {
                var type = FindType(typeName);
                if (type == null)
                {
                    return null;
                }

                var method = typeof(GameObject).GetMethod(
                    "FindObjectOfType",
                    BindingFlags.Public | BindingFlags.Static
                );

                if (method != null)
                {
                    var genericMethod = method.MakeGenericMethod(type);
                    var result = genericMethod.Invoke(null, null);
                    return result as GameObject;
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch
                {
                    // Ignore errors
                }
            }

            return null;
        }

        private static bool IsModElement(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            // Check for known mod naming patterns
            var name = go.name ?? string.Empty;
            return name.Contains("Alert") ||
                   name.Contains("Mod") ||
                   name.Contains("Custom") ||
                   name.StartsWith("LP_") ||
                   name.StartsWith("BLP_");
        }

        private static string GetUiNodeType(GameObject go)
        {
            if (go.GetComponent<Text>() != null)
            {
                return "text";
            }

            if (go.GetComponent<Button>() != null)
            {
                return "button";
            }

            if (go.GetComponent<Toggle>() != null)
            {
                return "toggle";
            }

            if (go.GetComponent<Slider>() != null)
            {
                return "slider";
            }

            if (go.GetComponent<Image>() != null)
            {
                return "image";
            }

            return "other";
        }

        private static bool IsInteractable(GameObject go)
        {
            var selectable = go.GetComponent<Selectable>();
            return selectable != null && selectable.interactable;
        }

        private static UiNodeData CreateNodeFromText(Text text, GameObject root)
        {
            return new UiNodeData
            {
                Path = BuildNodePath(text.transform, root.transform),
                Type = "text",
                Value = text.text ?? string.Empty,
                Interactable = false
            };
        }

        private static UiNodeData CreateNodeFromButton(Button button, GameObject root)
        {
            var buttonText = button.GetComponentInChildren<Text>();
            return new UiNodeData
            {
                Path = BuildNodePath(button.transform, root.transform),
                Type = "button",
                Value = buttonText != null ? buttonText.text : button.name,
                Interactable = button.interactable
            };
        }

        private static UiNodeData CreateNodeFromToggle(Toggle toggle, GameObject root)
        {
            _ = toggle.GetComponentInChildren<Text>();
            return new UiNodeData
            {
                Path = BuildNodePath(toggle.transform, root.transform),
                Type = "toggle",
                Value = toggle.isOn.ToString().ToLowerInvariant(),
                Interactable = toggle.interactable
            };
        }

        private static UiNodeData CreateNodeFromSlider(Slider slider, GameObject root)
        {
            return new UiNodeData
            {
                Path = BuildNodePath(slider.transform, root.transform),
                Type = "slider",
                Value = slider.value.ToString("F2"),
                Interactable = slider.interactable
            };
        }

        #endregion

        #region Internal Types

        private sealed class WindowInfo
        {
            public string Name { get; set; }
            public string WindowType { get; set; }
            public bool IsOpen { get; set; }
            public GameObject GameObject { get; set; }
        }

        #endregion
    }
}
