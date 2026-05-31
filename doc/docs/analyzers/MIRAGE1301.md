# MIRAGE1301: Message or RPC Class Warning

## The Problem
A field or property in a class/struct marked with `[NetworkMessage]`, or a parameter/return type argument in a method marked with `[ServerRpc]` or `[ClientRpc]`, uses a class type instead of a value type/struct.

Class-based types are generally risky for network messaging because:
1. **Allocations:** Mirage must allocate a new object instance upon deserialization, which causes garbage collection (GC) spikes and performance overhead.
2. **Polymorphism Limitations:** Mirage's standard serialization only serializes fields of the declared member type, not the concrete derived subclass type.

*Note: Standard collections (such as `List<T>` or `Dictionary<K, V>` in `System.Collections.Generic`) are automatically ignored by this rule as they are natively supported by Mirage's Weaver and serialization system.*

---

## Example of Triggering Code
```csharp
using Mirage;

public class TargetInfo
{
    public int x;
    public int y;
}

[NetworkMessage]
public struct FireMessage
{
    // Warns: NetworkMessage field 'info' is a class type 'TargetInfo'.
    public TargetInfo info;
}
```

---

## How to Resolve

### Option 1: Use a struct (Recommended)
Structs (value types) avoid memory allocations and guarantee safe copy-by-value semantics.
```csharp
public struct TargetInfo
{
    public int x;
    public int y;
}

[NetworkMessage]
public struct FireMessage
{
    public TargetInfo info;
}
```

### Option 2: Implement Custom Serialization and mark the class as safe
If you want to use the class type and manage performance/reference safety yourself, write custom `Write` and `Read` extension methods for the class, and decorate the class with `[WeaverSafeClass]` to suppress the warning globally.
```csharp
[WeaverSafeClass]
public class TargetInfo
{
    public int x;
    public int y;
}
```

### Option 3: Suppress the warning on the member or parameter
If you want to disable the warning only on a specific field, property, or parameter, decorate it with `[WeaverSafeClass]`.
```csharp
[NetworkMessage]
public struct FireMessage
{
    [WeaverSafeClass]
    public TargetInfo info;
}
```
