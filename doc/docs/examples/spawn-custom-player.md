---
sidebar_position: 20
---

# Spawn Custom Player Example

The SpawnCustomPlayer sample contains the full scripts for the [Custom Character Spawning](/docs/guides/game-objects/spawn-player-custom) guide. It is not a playable scene. It shows how to let each client choose its character (race, name, colors) and have the server spawn the matching prefab.

## What's Included

- **`CreateMMOCharacterMessage`**: A `[NetworkMessage]` the client sends with its choices: `race`, `name`, `hairColor` and `eyeColor`.
- **`CustomCharacterSpawner`**: Handles both sides:
  - **Client**: registers all character prefabs on the `ClientObjectManager` when the client starts, and sends `CreateMMOCharacterMessage` once authenticated.
  - **Server**: receives the message, picks the prefab for the chosen race, sets the SyncVars from the message, and calls `ServerObjectManager.AddCharacter`.
- **`CustomCharacter`**: The character `NetworkBehaviour`, with SyncVars for the name and colors.

```csharp
private void OnCreateCharacter(INetworkPlayer player, CreateMMOCharacterMessage msg)
{
    var prefab = GetPrefab(msg);

    // create your character object
    // use the data in msg to configure it
    var character = Instantiate(prefab);

    // set syncVars before telling mirage to spawn character
    // this will cause them to be sent to client in the spawn message
    character.PlayerName = msg.name;
    character.hairColor = msg.hairColor;
    character.eyeColor = msg.eyeColor;

    // spawn it as the character object
    ServerObjectManager.AddCharacter(player, character.Identity);
}
```

## Using It

1. Import the **SpawnCustomPlayer** sample.
2. Create a player prefab for each race with a `NetworkIdentity` and `CustomCharacter`.
3. Add `CustomCharacterSpawner` to your network GameObject, and assign the `NetworkClient`, `NetworkServer`, `ClientObjectManager`, `ServerObjectManager` and the 3 prefabs.
4. Remove or disable any other spawner (for example the player prefab on `NetworkSceneLoader`), so players aren't spawned twice.

See the [Custom Character Spawning](/docs/guides/game-objects/spawn-player-custom) guide for a step by step explanation.
