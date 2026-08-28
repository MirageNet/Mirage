---
sidebar_position: 4
---

# ChangeScene Example

The ChangeScene example demonstrates how to perform server-authoritative active scene transitions and client additive scene loading using `NetworkSceneLoader`.

![ChangeScene Example](/img/examples/change-scene/ChangeScene.png)

## How It Works

### `NetworkSceneLoader`
The `NetworkManager` GameObject uses `NetworkSceneLoader` to coordinate scene switching across the server and connected clients:

- **Server Scene Switching**: When the server calls `SceneLoader.ServerLoadScene("Room1")`, it unloads the previous active scene and loads the new scene for all connected players.
- **Client Synchronization**: Connected clients receive notification of the scene change, load the target scene, and send a ready confirmation back to the server once loading completes.
- **Automatic Character Spawning**: When players transition to the new scene and report ready, `NetworkSceneLoader` automatically respawns their player characters.

### `SceneSwitcherHud`
`SceneSwitcherHud` provides UI buttons to transition between active scenes and toggle additive sub-scenes:

- **Active Scene Changing**: Clicking **Room 1** or **Room 2** calls `SceneLoader.ServerLoadScene(...)` on the server.
- **Additive Sub-Scene Loading**: Clicking **Additive Load / Unload** sends an `AdditiveSceneMessage` to all authenticated players (`SceneLoader.Server.AuthenticatedPlayers`).
- **Host Mode Optimization**: Because host mode runs both server and client in the same process, `HandleAdditiveSceneMessage` checks `SceneLoader.Client.IsHost` to avoid redundantly loading or unloading the additive scene twice.

```csharp
private void HandleAdditiveSceneMessage(AdditiveSceneMessage msg)
{
    // Skip in host mode since the server already loaded or unloaded the scene locally
    if (SceneLoader.Client.IsHost)
        return;

    if (msg.Unload)
        SceneManager.UnloadSceneAsync(msg.ScenePath);
    else
        SceneManager.LoadSceneAsync(msg.ScenePath, LoadSceneMode.Additive);
}
```

## Running the Example

1. Add the following scenes to Unity's **Build Settings** in order:
   - `Scenes/Main.unity`
   - `Scenes/Room1.unity`
   - `Scenes/Room2.unity`
   - `Scenes/Additive.unity`
2. Open `Main.unity` and start **Host (Server + Client)** in the HUD.
3. Start a built instance and connect as **Client**.
4. Click **Room 1** or **Room 2** on the host to switch active scenes for all players.
5. Click **Additive Load** to overlay the additive scene on all clients.