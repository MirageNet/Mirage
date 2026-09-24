---
sidebar_position: 5
---
# SyncPrefab

## Overview

The `SyncPrefab` struct represents a prefab that is synchronized over the network. 

It has two properties:
- `Prefab`: A `NetworkIdentity` representing the prefab being synced.
- `PrefabHash`: An integer representing the hash of the prefab being synced. The `PrefabHash` is sent over the network so that the matching `NetworkIdentity` can be found on the other side.

`SyncPrefab` can be used to set up local objects like visual effects, audio, or projectiles, without needing to spawn them over the network, making it ideal for short-lived objects.

## Example Use Case

<!-- v2 -->
When the `Shoot` method is called on the server it will instantiates a local copy of the prefab. The `RpcShoot` is then called to send a message to all clients, Passing in a `SyncPrefab` object representing the prefab being synced.

On the client side, the `RpcShoot` method finds the prefab from `ClientObjectManager` using the `FindPrefab`. It then instantiates a local clone of the prefab. 

:::tip
Add `[NetworkedPrefab]` attribute to your inspector field to show if it is set up correctly.
:::

```cs
public class Shooter : NetworkBehaviour
 {
     // add [NetworkedPrefab] to ensure prefab is network object in inspector
     [NetworkedPrefab]
     public GameObject Prefab;

     // call this on server
     public void Shoot(Vector3 position, Quaternion rotation)
     {
         // spawn prefab locally
         var clone = Instantiate(Prefab, position, rotation);

         // then send to clients so they can also spawn locally

         RpcShoot(new SyncPrefab(Prefab.GetNetworkIdentity()), position, rotation);
     }

     [ClientRpc]
     public void RpcShoot(SyncPrefab syncPrefab, Vector3 position, Quaternion rotation)
     {
         // find prefab from objectManager
         var prefab = syncPrefab.FindPrefab(ClientObjectManager);

         // spawn prefab locally
         var clone = Instantiate(prefab, position, rotation);
     }
 }
```
