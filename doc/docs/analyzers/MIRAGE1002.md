# MIRAGE1002: SyncVar Auto-Property Error

## The Problem
A property marked with `[SyncVar]` is not configured as an automatic property, is static, or lacks a getter/setter.

Mirage's IL Weaver post-processes compiled assemblies by intercepting property writes to set dirty flags and run synchronization hooks. For this injection to succeed, the property **must** be a non-static automatic property (e.g. `public int Health { get; set; }`) so that the weaver can replace the compiler-generated backing field reads and writes.

---

## Example of Triggering Code
```csharp
public class Player : NetworkBehaviour
{
    private int _health;

    // Errors: SyncVar property 'health' must be a non-static auto-property...
    [SyncVar]
    public int health
    {
        get => _health;
        set => _health = value;
    }
}
```

---

## How to Resolve

Change the property to a standard auto-property with both `get` and `set` accessors.
```csharp
public class Player : NetworkBehaviour
{
    [SyncVar]
    public int health { get; set; }
}
```
