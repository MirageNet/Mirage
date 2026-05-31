# MIRAGE1401: Accessing Network State in Awake/Start

## The Problem
Reading or writing network states (such as `IsServer`, `IsClient`, `HasAuthority`, or `SyncVar` values) inside Unity's lifecycle methods `Awake` or `Start`.

Unity's `Awake` and `Start` methods are called during GameObject initialization. At this point, Mirage's network identity is not yet spawned or initialized, meaning properties like `IsServer`, `IsClient`, and authority states are not set, and `SyncVars` have not been initialized with their network values. Accessing them inside `Awake` or `Start` results in incorrect behavior, default values, or race conditions.

---

## Example of Triggering Code
```csharp
using Mirage;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    private void Start()
    {
        // Warning: Accessing Network State (IsServer/SyncVar) in Start
        if (IsServer)
        {
            Health = 100;
        }
    }
}
```

---

## How to Resolve

Override `OnStartServer`, `OnStartClient`, `OnStartLocalPlayer`, or `OnStartAuthority` to run network initialization code when the network state is fully ready.

```csharp
using Mirage;

public class PlayerHealth : NetworkBehaviour
{
    [SyncVar]
    public int Health { get; set; }

    // Correct: Run server initialization when the network server has started
    public override void OnStartServer()
    {
        Health = 100;
    }
}
```
