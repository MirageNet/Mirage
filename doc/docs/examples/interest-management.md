---
sidebar_position: 9
---

# Interest Management Example

The Interest Management example shows how to only send objects to the players that are near them. The server spawns 50 wandering NPCs and 500 pieces of loot. Each player only sees the objects within a few meters of their tank, and objects appear and disappear as they drive around.

This is how large worlds keep bandwidth down: clients don't need to know about things they can't see.

<!-- TODO screenshot: a tank with nearby NPCs and loot visible, and empty ground further away -->

## How It Works

### `NetworkProximityChecker`
The NPC, loot and tank prefabs each have a [`NetworkProximityChecker`](/docs/components/network-proximity-checker). On the server, every `VisibilityUpdateInterval` seconds it checks which players are within `VisibilityRange` of the object:

- When a player comes into range, the server sends that player a spawn message for the object.
- When the player moves out of range, the server tells the client to hide it.
- Players always see their own tank, even though it has a proximity checker.

| Prefab | `VisibilityRange` |
|---|---|
| Npc | 3 |
| Loot | 3 |
| Tank | 2 |

Try changing the range on the prefabs and see how much of the map each client can see.

### `Spawner`
The scene has two `Spawner`s, one for NPCs (50) and one for loot (500). When the server starts, each one spawns its prefab at random positions inside `bounds`:

```csharp
private void Awake()
{
    server.Started.AddListener(Spawn);
}

private void SpawnPrefab()
{
    var position = new Vector3(
        (Random.value - 0.5f) * bounds.size.x + bounds.center.x,
        (Random.value - 0.5f) * bounds.size.y + bounds.center.y,
        (Random.value - 0.5f) * bounds.size.z + bounds.center.z
    );

    var newLoot = GameObject.Instantiate(prefab, position, Quaternion.identity, transform);

    serverObjectManager.Spawn(newLoot);
}
```

The objects are spawned on the server only. Clients receive them later, when the proximity checker decides they are close enough.

### `Wander`
NPCs move to a random point every few seconds using a `NavMeshAgent`. `Wander` only runs on the server: it starts moving in `Identity.OnStartServer`, and the NPC's `NetworkTransform` sends the position to the clients that can see it.

### `PlayerSpawner` and `Tank`
`PlayerSpawner` spawns a tank for each player at the next spawn point, and gives it a random name before `AddCharacter`, so the name is included in the spawn message.

The tank's `NetworkTransform` has **Client Authority** enabled, so each player drives their own tank and the position is sent to the server.

## Running the Example

1. Import the **InterestManagement** sample, then open `Scenes/Scene.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD.
3. Start a built instance and click **Client**.
4. Use **W** and **A**/**D** (or the arrow keys) to drive. NPCs and loot appear as you get close and disappear as you drive away.

:::tip
Compare the Hierarchy on the host and on a client. The host has every NPC and piece of loot, while the client only has the ones near its tank.
:::
