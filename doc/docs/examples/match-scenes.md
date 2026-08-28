---
sidebar_position: 10
---

# Match Scenes Example

The Match Scenes example puts players into separate 2-player matches on one server. Each match gets its own copy of the match scene, loaded additively on the server with its own physics scene. Players only see the other player in their own match.

It shows how a single server can run many small games at the same time, for example for a 1v1 game.

<!-- TODO screenshot: two clients side by side, each showing only their own match (red and green player) -->

## How It Works

### `MatchScenesNetworkManager`
`MatchScenesNetworkManager` extends `NetworkManager` and handles matchmaking, scene loading and spawning.

**Server**
- **Matchmaking**: When a player is authenticated, they join the last match if it has 1 player, otherwise a new match is created. A match is full at 2 players.
- **One scene per match**: Creating a match loads a new copy of `MatchScenes_match` additively, with `LocalPhysicsMode.Physics3D`, so each match has its own physics.
- **Spawning**: When the client says it has loaded the scene (`SceneLoaded` message), the server creates the character, places it on the left or right, sets its color, moves it into the match's scene, and calls `AddCharacter`. If the match scene is still loading on the server, spawning waits until it has finished.
- **Cleanup**: When both players of a full match have left, the match scene is unloaded.

```csharp
private void AddCharacter(Scene scene, INetworkPlayer player, int v)
{
    var clone = Instantiate(_character);

    // make it easier to tell players apart
    clone.transform.position = new Vector3(-2 + (4 * v), 0, 0);
    var color = v == 0 ? Color.red : Color.green;
    var renderer = clone.GetComponentInChildren<Renderer>();
    renderer.material.color = color;
    var matchPlayer = clone.GetComponent<MatchScenesPlayer>();
    matchPlayer.color = color;

    var sceneChecker = (SceneVisibilityChecker)clone.Visibility;
    sceneChecker.MoveToScene(scene);
    ServerObjectManager.AddCharacter(player, clone);
}
```

**Client**
- When authenticated, the client loads `MatchScenes_match` as its only scene (clients only ever need their own match), then sends `SceneLoaded` to the server.
- In host mode this load is skipped, because the host shares the server's scenes.
- When disconnected, the client loads `MatchScenes_offline` again.

### `SceneVisibilityChecker`
The player prefab uses `SceneVisibilityChecker`, which only shows an object to players whose character is in the same scene. `MoveToScene` moves the character into the match's scene before it is spawned, so the first spawn message only goes to players in that match.

### `MatchScenesPlayer`
The color is a `[SyncVar]`, set before `AddCharacter` so it is part of the spawn message. Clients apply it to the renderer in `OnStartClient`.

## Running the Example

1. Import the **MatchScenes** sample.
2. In **Build Settings**, add both scenes from the sample folder:
   - `MatchScenes_offline.unity`
   - `MatchScenes_match.unity`
3. Open `MatchScenes_offline.unity` and build the project.
4. Press **Play** in the Editor and click **Server Only** (or **Host**).
5. Start built instances and click **Client**. Every 2 players are put into their own match, the first as red and the second as green. A 3rd and 4th player start a new match and can't see the first one.
