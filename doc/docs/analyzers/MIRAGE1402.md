# MIRAGE1402: Missing base Call in OnSerialize/OnDeserialize

## The Problem
Overriding `OnSerialize` or `OnDeserialize` in a derived `NetworkBehaviour` class without calling `base.OnSerialize` or `base.OnDeserialize`.

Derived classes that inherit from another `NetworkBehaviour` which has its own synchronized state must call the base implementation. Failing to call the base method prevents the base class's properties and SyncVars from being serialized or deserialized, leading to out-of-sync states between the server and clients.

---

## Example of Triggering Code
```csharp
using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    // Warning: Overriding OnSerialize without calling base.OnSerialize
    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        writer.WritePackedInt32(HeroId);
        return true;
    }
}
```

---

## How to Resolve

Add the call to `base.OnSerialize` or `base.OnDeserialize` inside the overridden method and combine its return value with the derived class's serialization status.

```csharp
using Mirage;
using Mirage.Serialization;

public class BasePlayer : NetworkBehaviour
{
    [SyncVar]
    public string PlayerName { get; set; }
}

public class HeroPlayer : BasePlayer
{
    [SyncVar]
    public int HeroId { get; set; }

    // Correct: Calls base.OnSerialize and combines dirty states
    public override bool OnSerialize(NetworkWriter writer, bool initialState)
    {
        bool baseDirty = base.OnSerialize(writer, initialState);
        writer.WritePackedInt32(HeroId);
        return baseDirty || true;
    }
}
```
