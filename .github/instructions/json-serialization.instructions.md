---
description: "Use when creating or modifying classes that will be serialized with Unity's JsonUtility. Enforces field-only data classes and the placeholder pattern for polymorphic serialization."
applyTo: "**/*.cs"
---

# JsonUtility Serialization Rules

Unity's `JsonUtility` cannot serialize dictionaries, anonymous objects, or properties. All JSON-serializable types must follow these rules.

## Data Classes

Every class serialized by `JsonUtility` must:

1. Be marked `[Serializable]`
2. Use **public lowercase fields only** — no properties
3. Suppress `CA1051` with `[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]`
4. Use only supported types: `string`, `int`, `float`, `double`, `bool`, `long`, `byte`, enum types, arrays of serializable types, or nested `[Serializable]` classes

```csharp
[Serializable]
[SuppressMessage("Design", "CA1051:Do not declare visible instance fields")]
public class GameStateData
{
    public int day;
    public string gameState;
    public float energy;
    public bool isPaused;
}
```

### Do NOT

- Add PascalCase property accessors that shadow lowercase fields — this causes serialization failures
- Use `Dictionary<K, V>` on types serialized outbound (only works for inbound deserialization)
- Use anonymous objects or LINQ projections as serialization targets
- Use `List<T>` — prefer arrays (`T[]`) for JsonUtility compatibility

## Placeholder Serialization Pattern

When a response must carry an arbitrary typed object (polymorphic data), use the placeholder trick from `MessageSerializer.cs`:

1. Declare a `string` field on the wrapper to hold a placeholder value
2. Store the actual object in a `[NonSerialized] object` field
3. Serialize the wrapper with a placeholder string, serialize the data object separately, then string-replace the placeholder with the real JSON

```csharp
// In the wrapper class
public string data; // Placeholder field for JsonUtility
[NonSerialized]
public object DataObject; // Actual data, not serialized directly

// In the serializer
response.data = "__DATA_PLACEHOLDER__";
string wrapperJson = JsonUtility.ToJson(response);
string dataJson = JsonUtility.ToJson(response.DataObject);
return wrapperJson.Replace("\"__DATA_PLACEHOLDER__\"", dataJson);
```

Reference implementations: `Protocol/Response.cs`, `Protocol/MessageSerializer.cs`.
