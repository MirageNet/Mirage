---
sidebar_position: 7
---

# Multiple Additive Scenes Example

The Multiple Additive Scenes example runs several isolated game instances on a single server. Each instance is an additive copy of the `Game` scene with its own physics scene, and `NetworkSceneChecker` makes sure players only see objects in their own instance.

<!-- TODO screenshot: game view with scores at the top, a few players and prizes in one instance -->

## How It Works

### `MultiSceneNetManager`
`MultiSceneNetManager` extends `NetworkManager` and handles scene loading and character spawning. Its scene loading is based on [`NetworkSceneLoader`](/docs/guides/scene-loading/network-scene-loader), changed to load scenes additively:

- **Server Loading**: When the server starts, it loads `Instances` copies of the `Game` scene with `LocalPhysicsMode.Physics3D`, then calls `ServerObjectManager.SpawnSceneObjects()` to spawn the networked objects in them.
- **Client Loading**: When a client is authenticated, the server sends it a `SceneMessage`. The client loads a single copy of the `Game` scene (without `localPhysicsMode`), calls `ClientObjectManager.PrepareToSpawnSceneObjects()`, and replies with `SceneReadyMessage`.
- **Character Spawning**: Once the client is ready (and the server has finished loading), the server creates the character at the next spawn point, sets its `PlayerScore` SyncVars, moves it into one of the game instances, and then calls `ServerObjectManager.AddCharacter()`.
- **Host**: The host shares the server's scenes, so it skips the client loading step and is ready right away.

```csharp
private void SpawnCharacterForPlayer(INetworkPlayer player)
{
    var spawnPoint = SpawnPoints[_nextSpawnIndex];
    _nextSpawnIndex = (_nextSpawnIndex + 1) % SpawnPoints.Count;
    var character = Instantiate(PlayerPrefab, spawnPoint.position, spawnPoint.rotation);

    // increment first, so first player has id=1
    _nextPlayerId++;
    var matchIndex = _nextPlayerId % _subScenes.Count;

    // Set SyncVars before AddCharacter so they are included in the spawn message
    var playerScore = character.GetComponent<PlayerScore>();
    playerScore.playerNumber = _nextPlayerId;
    playerScore.scoreIndex = _nextPlayerId / _subScenes.Count;
    playerScore.matchIndex = matchIndex;

    // Move to sub scene before spawning, so NetworkSceneChecker only shows it to players in the same match
    SceneManager.MoveGameObjectToScene(character.gameObject, _subScenes[matchIndex]);

    ServerObjectManager.AddCharacter(player, character.gameObject);
}
```

### Physics and Isolation
- Physics scenes do not auto-simulate, so each `Game` scene has a `PhysicsSimulator` that steps its physics scene on the server. It does nothing on the client.
- All networked objects have a `NetworkSceneChecker`, which only shows them to players whose character is in the same scene.

## Running the Example

1. Import the **MultipleAdditiveScenes** sample.
2. In **Build Settings**, add the scenes from the sample's `Scenes` folder in order:
   - `Scenes/Main.unity`
   - `Scenes/Game.unity`
3. Open `Main.unity`, press **Play** and click **Host (Server + Client)** in the HUD. You can also use **Server Only**.
4. Start at least 3 built instances and click **Client**.
5. Use **WASDQE** to move and turn your player capsule, and **Space** to jump.
   - Colliding with the small colored spheres scores points based on their color.
   - Colliding with the larger tumblers sends them rolling around. They are server-side non-kinematic rigidbodies.
   - Only scores for players in the same instance are shown at the top of the game window.
