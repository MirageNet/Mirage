---
title: NetworkSceneLoader
---
# NetworkSceneLoader

`NetworkSceneLoader` is a ready-made component in `Mirage.Components` that implements the **"Join Any Time"** scene loading pattern. It handles the synchronization between server and client, ensuring characters are spawned only after both sides have finished loading the scene.

For more complex use cases, it is best to create a copy of this script and modify it for your needs. See [Manual Scene Loading](/docs/guides/scene-loading/manual-scene-loading) for the full step-by-step breakdown.

## Setup

Add the `NetworkSceneLoader` component to the same GameObject as your `NetworkServer` and `NetworkClient`. It requires the following references:

| Field | Description |
|---|---|
| **Server** | Reference to the `NetworkServer` component |
| **Client** | Reference to the `NetworkClient` component |
| **ServerObjectManager** | Reference to the `ServerObjectManager` component |
| **ClientObjectManager** | Reference to the `ClientObjectManager` component |
| **PlayerPrefab** | The `NetworkIdentity` prefab to spawn as the player's character |

:::note
These references will be auto-found via `OnValidate` if they are on the same GameObject.
:::

## Usage

To load a new scene for all connected players:

```cs
public class MyGameManager : MonoBehaviour
{
    public NetworkSceneLoader SceneLoader;

    public void StartGame()
    {
        SceneLoader.ServerLoadScene("BattleMap").Forget();
    }
}
```

## How It Works

`NetworkSceneLoader` implements the "Join Any Time" pattern described in [Manual Scene Loading](/docs/guides/scene-loading/manual-scene-loading). Here is what happens when you call `ServerLoadScene`:

**ServerLoadScene**
1. Sets `ServerLoading = true` and stores the `TargetScene`
2. Loops over all connected players, marks them as not ready, and sends a `SceneMessage`
3. Loads the scene on the server
4. In host mode, calls `PrepareToSpawnSceneObjects` and marks the local player as ready
5. Sets `ServerLoading = false` and calls `SpawnSceneObjects`
6. Spawns characters for any players that are already ready

**OnServerAuthenticated** (late joiners)
- If a `TargetScene` is set, sends a `SceneMessage` to the new player
- Host player is skipped (handled by server loading logic)

**HandleSceneReadyMessage**
- Marks the player as ready
- If the server has finished loading (`ServerLoading == false`), spawns their character immediately

**Client side**
- Receives `SceneMessage`, loads the scene, calls `PrepareToSpawnSceneObjects`, then sends `SceneReadyMessage` back
- In host mode, scene loading is skipped (the server already loaded it)

## Creating a Custom Loader (Modifying NetworkSceneLoader)

The default `NetworkSceneLoader` component only implements the basic **Join Any Time** pattern. It is intentionally designed as a starting point or template rather than an all-in-one system.

If your game needs different behavior, **do not edit the script directly inside the package**. Instead:
1. Duplicate `NetworkSceneLoader.cs` from the Mirage package folder into your own assets.
2. Rename the class (e.g. `MyCustomSceneLoader`) and modify the code.

Here are some common customization ideas and code recipes you can copy-paste into your duplicate loader script:

### Custom Spawn Points
The default template simply instantiates the prefab at `Vector3.zero`. You can modify `SpawnCharacterForPlayer` to locate spawn points or players' start positions within the loaded scene.

### Player Proxy Pattern
Instead of spawning a gameplay character directly, spawn a lightweight persistent proxy object (like a `PlayerContext`) and grant player authority over gameplay characters when entering a match. See the [Player Proxy Pattern](/docs/guides/game-objects/player-proxy-pattern) guide for more details.

### Recipe: Fixed Match Size
For games where all players must start at the exact same time (e.g., a competitive round or Battle Royale), you can modify the `HandleSceneReadyMessage` method inside your duplicated class. 

Instead of immediately spawning a character when *one* player is ready, check if *all* players are ready first:

```cs
// Modify this inside your duplicated class
private void HandleSceneReadyMessage(INetworkPlayer player, SceneReadyMessage message)
{
    player.SceneIsReady = true;

    // Check if everyone has finished loading
    bool allReady = true;
    foreach (var p in Server.AllPlayers)
    {
        if (!p.SceneIsReady)
        {
            allReady = false;
            break;
        }
    }

    // Only spawn characters once everyone is fully loaded
    if (allReady)
    {
        foreach (var p in Server.AllPlayers)
            SpawnCharacterForPlayer(p);
    }
}
```

