# MIRAGE1502: Unbounded String or Collection

## The Problem
A network message field or RPC parameter contains a `string`, `List<T>`, `T[]` array, or other collection without specifying a maximum size/length limit.

Allowing unbounded strings or collections in network messages introduces security risks (such as memory exhaustion attacks, out-of-memory crashes, or denial of service) if a client sends an extremely large payload.

---

## Example of Triggering Code
```csharp
using Mirage;

[NetworkMessage]
public struct ChatMessage
{
    // Warning: Unbounded string can be exploited to send megabytes of text
    public string text;
}
```

---

## How to Resolve

Use size-limiting attributes (such as `[BitCount]` or other string/collection size limiters) to restrict the collection size at serialization time, or enforce maximum limits during deserialization (such as setting `MaxDeltaCount` or `MaxElements` on SyncObjects).

```csharp
using Mirage;
using Mirage.Serialization;

[NetworkMessage]
public struct ChatMessage
{
    // Correct: Restrict the maximum string length using BitCount or other validation attributes
    [BitCount(8)]
    public string text;
}
```
