// SPDX-License-Identifier: MIT

#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Hemocode.Playwright.Queries
{
    /// <summary>
    /// Queries for discovering, inspecting, and searching GameObjects in the active scene.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class GameObjectQueries
    {
        private const int DefaultDepth = 3;
        private const int MaxSearchResults = 500;
        private const int MaxDumpDepth = 10;
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
        /// Discovers the GameObject hierarchy of the active scene.
        /// </summary>
        /// <param name="depth">Maximum depth to traverse (default: 3, max: 10).</param>
        /// <param name="dumpPath">Optional file path to write JSON dump to.</param>
        /// <returns>List of root GameObjectNodeData nodes.</returns>
        public static ICollection<GameObjectNodeData> Discover(int depth = DefaultDepth, string dumpPath = null)
        {
            EnsureMainThread();

            var clampedDepth = Math.Min(Math.Max(depth, 1), MaxDumpDepth);
            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var roots = new List<GameObjectNodeData>();

            foreach (var root in rootObjects)
            {
                if (root != null)
                {
                    roots.Add(BuildNode(root.transform, 0, clampedDepth));
                }
            }

            if (!string.IsNullOrEmpty(dumpPath))
            {
                WriteDump(roots, dumpPath);
            }

            return roots;
        }

        /// <summary>
        /// Inspects a specific GameObject by its hierarchy path.
        /// </summary>
        /// <param name="path">Hierarchy path to the GameObject (e.g., "Canvas/Panel").</param>
        /// <param name="detail">"summary" for component names only, "full" for reflected fields.</param>
        /// <returns>Detailed inspection data, or null if not found.</returns>
        public static GameObjectInspectData Inspect(string path, string detail = "summary")
        {
            EnsureMainThread();

            var go = GameObject.Find(path);
            if (go == null)
            {
                return null;
            }

            var components = ReflectComponents(go, detail);
            var tag = "Untagged";
            try
            {
                tag = go.tag;
            }
            catch
            {
                // Tag access can throw if tag is not defined
            }

            return new GameObjectInspectData
            {
                Name = go.name,
                Path = GetGameObjectPath(go.transform),
                Active = go.activeInHierarchy,
                Tag = tag,
                Layer = go.layer,
                Components = components
            };
        }

        /// <summary>
        /// Searches for GameObjects matching the specified criteria.
        /// </summary>
        /// <param name="name">Name substring to match (case-insensitive).</param>
        /// <param name="nameMatch">"contains" (default) or "exact".</param>
        /// <param name="component">Component type name to filter by.</param>
        /// <param name="tag">Tag to filter by.</param>
        /// <param name="activeOnly">If true, only return active GameObjects.</param>
        /// <returns>Search results capped at MaxSearchResults.</returns>
        public static GameObjectSearchResult Search(
            string name = null,
            string nameMatch = "contains",
            string component = null,
            string tag = null,
            bool? activeOnly = null)
        {
            EnsureMainThread();

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var results = new List<GameObjectNodeData>();
            var queryCriteria = new List<string>();

            if (!string.IsNullOrEmpty(name))
            {
                queryCriteria.Add($"name {nameMatch} \"{name}\"");
            }

            if (!string.IsNullOrEmpty(component))
            {
                queryCriteria.Add($"component={component}");
            }

            if (!string.IsNullOrEmpty(tag))
            {
                queryCriteria.Add($"tag={tag}");
            }

            if (activeOnly == true)
            {
                queryCriteria.Add("activeOnly");
            }

            var queryDescription = queryCriteria.Count > 0
                ? string.Join(", ", queryCriteria.ToArray())
                : "all";

            foreach (var go in allObjects)
            {
                if (results.Count >= MaxSearchResults)
                {
                    break;
                }

                if (go == null)
                {
                    continue;
                }

                if (!MatchesFilter(go, name, nameMatch, component, tag, activeOnly))
                {
                    continue;
                }

                var goTag = "Untagged";
                try
                {
                    goTag = go.tag;
                }
                catch
                {
                    // Tag access can throw if tag is not defined
                }

                var componentNames = GetComponentNames(go);

                results.Add(new GameObjectNodeData
                {
                    Name = go.name,
                    Path = GetGameObjectPath(go.transform),
                    Active = go.activeInHierarchy,
                    Tag = goTag,
                    Layer = go.layer,
                    Components = componentNames,
                    ChildCount = go.transform.childCount,
                    Children = null
                });
            }

            return new GameObjectSearchResult
            {
                Query = queryDescription,
                ResultCount = results.Count,
                Results = results
            };
        }

        /// <summary>
        /// Builds a GameObjectNodeData tree from a Transform, recursing to the specified depth.
        /// </summary>
        internal static GameObjectNodeData BuildNode(Transform t, int currentDepth, int maxDepth)
        {
            var go = t.gameObject;
            var tag = "Untagged";
            try
            {
                tag = go.tag;
            }
            catch
            {
                // Tag access can throw if tag is not defined
            }

            var componentNames = GetComponentNames(go);

            var node = new GameObjectNodeData
            {
                Name = go.name,
                Path = GetGameObjectPath(t),
                Active = go.activeInHierarchy,
                Tag = tag,
                Layer = go.layer,
                Components = componentNames,
                ChildCount = t.childCount,
                Children = new List<GameObjectNodeData>()
            };

            if (currentDepth < maxDepth)
            {
                for (var i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    if (child != null)
                    {
                        node.Children.Add(BuildNode(child, currentDepth + 1, maxDepth));
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Walks the parent chain to build a "/" delimited path for a Transform.
        /// </summary>
        internal static string GetGameObjectPath(Transform t)
        {
            if (t == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var current = t;

            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts.ToArray());
        }

        /// <summary>
        /// Enumerates components on a GameObject, optionally reflecting their fields.
        /// </summary>
        private static List<ComponentData> ReflectComponents(GameObject go, string detail)
        {
            var components = go.GetComponents<Component>();
            var result = new List<ComponentData>();

            foreach (var comp in components)
            {
                if (comp == null)
                {
                    continue;
                }

                var compData = new ComponentData
                {
                    TypeName = comp.GetType().FullName,
                    Fields = new List<ComponentFieldData>()
                };

                if (string.Equals(detail, "full", StringComparison.OrdinalIgnoreCase))
                {
                    var fields = comp.GetType().GetFields(
                        BindingFlags.Public | BindingFlags.Instance);

                    foreach (var field in fields)
                    {
                        try
                        {
                            var value = field.GetValue(comp);
                            compData.Fields.Add(new ComponentFieldData
                            {
                                Name = field.Name,
                                Type = field.FieldType.Name,
                                Value = value != null ? value.ToString() : "null"
                            });
                        }
                        catch
                        {
                            compData.Fields.Add(new ComponentFieldData
                            {
                                Name = field.Name,
                                Type = field.FieldType.Name,
                                Value = "<error reading value>"
                            });
                        }
                    }
                }

                result.Add(compData);
            }

            return result;
        }

        /// <summary>
        /// Gets component type names for a GameObject.
        /// </summary>
        private static string[] GetComponentNames(GameObject go)
        {
            var components = go.GetComponents<Component>();
            var names = new List<string>();
            foreach (var comp in components)
            {
                if (comp != null)
                {
                    names.Add(comp.GetType().Name);
                }
            }

            return names.ToArray();
        }

        /// <summary>
        /// Checks whether a GameObject matches the specified search filters.
        /// </summary>
        private static bool MatchesFilter(
            GameObject go,
            string name,
            string nameMatch,
            string component,
            string tag,
            bool? activeOnly)
        {
            if (activeOnly == true && !go.activeInHierarchy)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(name))
            {
                if (string.Equals(nameMatch, "exact", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.Equals(go.name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    if (go.name == null ||
                        go.name.IndexOf(name, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(component))
            {
                var hasComponent = false;
                var components = go.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp != null &&
                        comp.GetType().Name.IndexOf(component, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hasComponent = true;
                        break;
                    }
                }

                if (!hasComponent)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(tag))
            {
                try
                {
                    if (!string.Equals(go.tag, tag, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Writes a JSON dump of the GameObject tree using manual string building
        /// to avoid JsonUtility's 7-level depth limit on recursive types.
        /// </summary>
        private static void WriteDump(ICollection<GameObjectNodeData> roots, string dumpPath)
        {
            try
            {
                var dir = Path.GetDirectoryName(dumpPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    _ = Directory.CreateDirectory(dir);
                }

                var sb = new StringBuilder();
                _ = sb.Append("[\n");

                var rootIndex = 0;
                foreach (var root in roots)
                {
                    WriteNodeJson(sb, root, 1);
                    if (rootIndex < roots.Count - 1)
                    {
                        _ = sb.Append(',');
                    }

                    _ = sb.Append('\n');
                    rootIndex++;
                }

                _ = sb.Append(']');
                File.WriteAllText(dumpPath, sb.ToString());
            }
            catch (Exception ex)
            {
                Server.TcpServer.LogDebug($"[LobotomyPlaywright] Failed to write dump: {ex.Message}");
            }
        }

        private static void WriteNodeJson(StringBuilder sb, GameObjectNodeData node, int indent)
        {
            var pad = new string(' ', indent * 2);
            var innerPad = new string(' ', (indent + 1) * 2);

            _ = sb.Append(pad).Append("{\n");
            _ = sb.Append(innerPad).Append("\"name\": ").Append(EscapeJson(node.Name)).Append(",\n");
            _ = sb.Append(innerPad).Append("\"path\": ").Append(EscapeJson(node.Path)).Append(",\n");
            _ = sb.Append(innerPad).Append("\"active\": ").Append(node.Active ? "true" : "false").Append(",\n");
            _ = sb.Append(innerPad).Append("\"tag\": ").Append(EscapeJson(node.Tag)).Append(",\n");
            _ = sb.Append(innerPad).Append("\"layer\": ").Append(node.Layer).Append(",\n");

            // Components array
            _ = sb.Append(innerPad).Append("\"components\": [");
            if (node.Components != null && node.Components.Length > 0)
            {
                for (var i = 0; i < node.Components.Length; i++)
                {
                    _ = sb.Append(EscapeJson(node.Components[i]));
                    if (i < node.Components.Length - 1)
                    {
                        _ = sb.Append(", ");
                    }
                }
            }

            _ = sb.Append("],\n")
                .Append(innerPad).Append("\"childCount\": ").Append(node.ChildCount).Append(",\n");

            // Children array
            _ = sb.Append(innerPad).Append("\"children\": [");
            if (node.Children != null && node.Children.Count > 0)
            {
                _ = sb.Append('\n');
                var childIndex = 0;
                foreach (var child in node.Children)
                {
                    WriteNodeJson(sb, child, indent + 2);
                    if (childIndex < node.Children.Count - 1)
                    {
                        _ = sb.Append(',');
                    }

                    _ = sb.Append('\n');
                    childIndex++;
                }

                _ = sb.Append(innerPad);
            }

            _ = sb.Append("]\n")
                .Append(pad).Append('}');
        }

        private static string EscapeJson(string s)
        {
            if (s == null)
            {
                return "null";
            }

            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";
        }

        private static void EnsureMainThread()
        {
            if (s_mainThreadId == -1)
            {
                s_mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }

            if (!s_disableThreadCheck)
            {
                var currentThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if (currentThreadId != s_mainThreadId)
                {
                    throw new InvalidOperationException(
                        "GameObjectQueries must be called from the Unity main thread. " +
                        "Current thread ID: " + currentThreadId + ", Main thread ID: " + s_mainThreadId + ". " +
                        "Unity APIs are not thread-safe and will crash if called from background threads."
                    );
                }
            }
        }
    }
}
