# MIRAGE1101: Misplaced Network Attribute Error

## The Problem
A Mirage-specific attribute (such as `[SyncVar]`, `[Server]`, `[Client]`, `[ServerRpc]`, `[ClientRpc]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]`) is declared on a field, property, or method inside a class that does not inherit from `NetworkBehaviour`.

These attributes control network synchronization or inject runtime active checks (guards) that require the state and context of a `NetworkBehaviour` instance. Using them in standard `MonoBehaviour` or plain C# classes is invalid and will cause compile or weaver failures.

---

## Example of Triggering Code
```csharp
using UnityEngine;
using Mirage;

public class GameManager : MonoBehaviour
{
    // Errors: Attribute 'SyncVarAttribute' cannot be used on 'score'...
    [SyncVar]
    public int score { get; set; }
}
```

---

## How to Resolve

Ensure that the declaring class inherits from `NetworkBehaviour` instead of `MonoBehaviour` or other base classes.
```csharp
using Mirage;

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    public int score { get; set; }
}
```
