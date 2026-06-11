---
sidebar_position: 4
title: Spawn Player - Custom
---
# Custom Character Spawning

:::note
Full scripts for this page can be found in the SpawnCustomPlayer sample in the package manager or on [GitHub](https://github.com/MirageNet/Mirage/tree/main/Assets/Mirage/Samples%7E/SpawnCustomPlayer)
:::

Mirage comes with a CharacterSpawner which will automatically spawn a character object when a client connects.

Many games need character customization. You may want to pick the color of the hair, eyes, skin, height, race, etc.

In this case, you will need to create your own CharacterSpawner.  Follow these steps:

1) Create your player prefabs (as many as you need) and add them to the Spawnable Prefabs in your ClientObjectManager.
2) Create a message that describes your player. For example:
{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'create-mmo-character-message' }}}
3) Create Player Spawner class and add it to some GameObject in your scene
{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-class' }}}
4) Drag the NetworkClient and NetworkServer and Scene manager to the fields

5) Hook into events:

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-start' }}}

6) register the prefabs when the client starts

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-client-started' }}}

7) Send your message with your character data when your client connects, or after the user submits his preferences.

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-client-authenticated' }}}
8) Receive your message in the server and spawn the player

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-server-started' }}}

## Ready State

In Mirage, players have a `SceneIsReady` state. The server uses this to determine if a player has finished loading the scene and is ready to receive spawned objects and state updates. 

### Server Authority
By default, Mirage does not automatically spawn characters when a player connects. This is to prevent "Spawn Amplification" attacks and to give you full control over when a player enters the game.

The recommended flow is the **"Join Any Time"** pattern:
1. The Server tells the Client to load a scene using a `SceneMessage`.
2. The Client loads the scene and sends back a `SceneReadyMessage`.
3. The Server sets `player.SceneIsReady = true`.
4. Only once the player is ready, the Server calls `ServerObjectManager.AddCharacter`.

### Using NetworkSceneLoader
To simplify this process, you can use the [NetworkSceneLoader](/docs/guides/scene-loading/network-scene-loader) component provided in `Mirage.Components`. It implements this "Join Any Time" pattern for you, handling the messages and coordinating the character spawning once both the server and the client are synchronized on the same scene.

If you are implementing your own spawner from scratch (like the example above), you should manually check `player.SceneIsReady` before spawning, or wait for the `SceneReadyMessage` handler as shown in the guides.

## Switching Characters

To replace the character game object for a player, use `ServerObjectManager.ReplaceCharacter`. This is useful for having different game objects for the player at different times, such as in-game and a pregame lobby. The function takes the same arguments as `AddCharacter`, but allows there to already be a character for that player. The old character game object is not destroyed when ReplaceCharacter is called. The `NetworkRoomManager` uses this technique to switch from the `NetworkRoomPlayer` game object to a game-play player game object when all the players in the room are ready.

You can also use `ReplaceCharacter` to respawn a player or change the object that represents the player. In some cases, it is better to just disable a game object and reset its game attributes on respawn. The following code sample demonstrates how to replace the player game object with a new game object:

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-respawn' }}}


## Destroying Characters

Once the character is finished (eg game over, or player died) you can remove the character using `ServerObjectManager.DestroyCharacter`.

{{{ Path:'Snippets/GameObjects/CustomPlayerSpawning.cs' Name:'custom-character-spawner-death' }}}

Alternatively, you can use `ServerObjectManager.RemoveCharacter` to remove it as the player's character without destroying it.
