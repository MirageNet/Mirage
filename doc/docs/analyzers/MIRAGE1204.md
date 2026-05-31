# MIRAGE1204: Invalid ClientRpc Target Configurations

## The Problem
A `[ClientRpc]` target configuration is invalid for one of the following reasons:
1. The method return type is `UniTask` or `UniTask<T>` (it returns values) but the target is configured as `RpcTarget.Observers`.
2. The target is set to `RpcTarget.Player` but the first parameter of the method is not an `INetworkPlayer` (or `NetworkConnection`) to specify the recipient.

Broadcast RPCs (where the target is `Observers`) cannot collect return values since multiple clients would respond. Returning values requires a single, specific destination (e.g. `RpcTarget.Owner` or `RpcTarget.Player`). Furthermore, when targeting a specific `Player`, Mirage needs to know which connection to send the RPC to, so the method's first parameter must be the player connection.

---

## Example of Triggering Code
```csharp
using Mirage;
using Cysharp.Threading.Tasks;

public class Player : NetworkBehaviour
{
    // Error: [ClientRpc] must return void when target is Observers.
    [ClientRpc(target = RpcTarget.Observers)]
    public UniTask<int> RpcGetHealth()
    {
        return UniTask.FromResult(100);
    }

    // Error: ClientRpc method with target = Player requires first parameter to be INetworkPlayer
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcGiveItem(int itemId)
    {
    }
}
```

---

## How to Resolve

1. If the RPC returns values, change the target to `RpcTarget.Owner` or `RpcTarget.Player`.
2. If the RPC targets `RpcTarget.Player`, ensure the first parameter is of type `INetworkPlayer` (or `NetworkConnection`).

```csharp
using Mirage;
using Cysharp.Threading.Tasks;

public class Player : NetworkBehaviour
{
    // Correct: Targeted RPC returning value to the Owner
    [ClientRpc(target = RpcTarget.Owner)]
    public UniTask<int> RpcGetHealth()
    {
        return UniTask.FromResult(100);
    }

    // Correct: First parameter is the target player connection
    [ClientRpc(target = RpcTarget.Player)]
    public void RpcGiveItem(INetworkPlayer targetPlayer, int itemId)
    {
    }
}
```
