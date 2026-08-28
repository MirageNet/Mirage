---
sidebar_position: 21
---

# Send Prefab Example

The SendPrefab sample is a single script, not a playable scene. It shows how a client can ask the server to spawn an object, and how the server stays in control of what gets spawned.

## What's Included

**`SendPrefab`** is a `NetworkBehaviour` with an `AllowedPrefab` array. When the owner presses **Space**, it sends the *index* of a prefab to the server, and the server spawns it in front of the object and gives the client authority over it.

```csharp
// limit how often a client can spawn objects, so they can't flood the server
[RateLimit(Interval = 1f, Refill = 2, MaxTokens = 5)]
[ServerRpc]
private void CmdSpawnPrefab(int index)
{
    // Security: only a modified client could send an index outside the array, so kick them
    if (index < 0 || index >= AllowedPrefab.Length)
    {
        Owner.SetErrorAndDisconnect(PlayerErrorFlags.LikelyCheater);
        return;
    }

    // get the prefab from the index
    var prefab = AllowedPrefab[index];

    // get a position in front of this object.
    // calculated on the server, so the client can't spawn objects anywhere it wants
    var position = transform.position + (transform.forward * 2);

    // Instantiate and Spawn prefab
    var clone = Instantiate(prefab, position, Quaternion.identity);

    // spawn the new object and give authority to the player who called the RPC
    ServerObjectManager.Spawn(clone, Owner);
}
```

Things this shows:

- **Never send a prefab, send an index**: The client picks from a list the server already has, so it can only spawn objects you allowed.
- **Validate client input**: A bad index means the client was modified, so the server disconnects it with `SetErrorAndDisconnect`.
- **Server decides the position**: The server works out where to spawn, instead of trusting a position from the client.
- **Rate limiting**: `[RateLimit]` stops a client from spamming spawns. See [Rate Limiting](/docs/guides/remote-actions/rate-limiting).
- **Authority**: `ServerObjectManager.Spawn(clone, Owner)` gives the client that asked authority over the new object.

## Using It

1. Import the **SendPrefab** sample.
2. Add `SendPrefab` to your player prefab.
3. Fill `AllowedPrefab` with prefabs that have a `NetworkIdentity`, and register them on the `ClientObjectManager`.
4. Play as host or client and press **Space**.
