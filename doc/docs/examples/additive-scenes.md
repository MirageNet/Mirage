---
sidebar_position: 6
---

# Additive Scenes Example

The Additive Scenes example demonstrates spatial sub-scene loading and unloading based on trigger zones, network entity animation via RPCs, and interest management using a proximity checker that also checks if the player has loaded the sub-scene.

<!-- TODO screenshot: player standing inside the shelter zone with the SubScene shapes visible -->

## How It Works

### `PlayerSpawner`
`PlayerSpawner` spawns a character for each player as soon as they are authenticated. It is a copy of [`NetworkSceneLoader`](/docs/guides/scene-loading/network-scene-loader) with scene changing removed (players stay in the starting scene) and spawn points added:

- **Immediate Client Readiness**: The client is already in the starting scene, so it is marked ready on authentication (`player.SceneIsReady = true`).
- **Spawn Points**: Each new character is placed at the next transform in the `SpawnPoints` list, looping back to the first one.

### Sub-Scene Loading (`SubSceneLoader` and `ZoneHandler`)
`SubSceneLoader` is based on [`NetworkSceneLoader`](/docs/guides/scene-loading/network-scene-loader), but loads `SubScene` additively for each player instead of changing the scene for everyone:

- **Server Loading**: When the server starts, it loads `SubScene` additively and calls `ServerObjectManager.SpawnSceneObjects()`, so the objects inside it exist on the server.
- **Zone Trigger**: `ZoneHandler` is on a server-only trigger collider in `MainScene`. When a player enters or leaves it, the server calls `ServerShowSubScene` or `ServerHideSubScene` for that player.
- **Client Loading**: The server sends a `SubSceneMessage` to the player. The client loads `SubScene` additively, calls `ClientObjectManager.PrepareToSpawnSceneObjects()`, and replies with `SubSceneLoadedMessage`.
- **Showing Objects**: Only after that reply does the server mark the player as loaded and rebuild the observers of the sub-scene objects, so it never sends a spawn message for an object the client can't find.
- **Unloading**: When the player leaves the zone, the server first hides the sub-scene objects from them, then tells the client to unload `SubScene`.
- **Load/Unload Races**: The client keeps loading or unloading until it matches the last message from the server, so leaving the zone while the scene is still loading works.

### Visibility (`SubSceneProximityChecker`)
The objects in the scenes use `SubSceneProximityChecker`, which extends `NetworkProximityChecker`:

- Objects in `MainScene` are shown to players within `VisibilityRange`, the same as `NetworkProximityChecker`.
- Objects in `SubScene` are also hidden from players that have not loaded `SubScene`.

### Turret Targeting & Rate-Limited RPCs (`ShootingTankBehaviour`)
In `MainScene`, an automated turret tracks nearby players and fires projectiles:

- **Target Acquisition**: The turret rotates towards the nearest player within its observation range.
- **Throttled Firing**: Firing is throttled using a `fireInterval` cooldown (e.g. 1.5s) to avoid spamming network calls every frame:

```csharp
if (Time.time >= _nextFireTime)
{
    _nextFireTime = Time.time + fireInterval;

    // Modern approach: trigger animation via ClientRpc
    RpcFire();
}
```

- **Animation RPC**: Calling `RpcFire()` invokes a `[ClientRpc]` on all observers to trigger the firing animation and sound effects.

## Running the Example

> [!IMPORTANT]
> Make sure your Unity project has a layer named `Player` configured in Project Settings -> Tags and Layers for trigger detection to work properly.

1. Add the scenes to Unity's **Build Settings** in order:
   - `Scenes/MainScene.unity`
   - `Scenes/SubScene.unity`
2. Open `MainScene.unity` and start **Host (Server + Client)** in the HUD.
3. Start up to 3 built client instances and connect as **Client**.
4. Use **WASDQE** to move and turn your player capsule.
5. Move towards the central shelter (the trigger zone). When entering the zone:
   - The client will additively load `SubScene`, and the networked shapes inside it will appear once they are within range.
   - When exiting the shelter area, the shapes are hidden and the sub-scene is unloaded.
