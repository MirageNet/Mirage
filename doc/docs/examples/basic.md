---
sidebar_position: 2
---

# Basic Example

The Basic example is a minimal baseline demonstrating single-scene player connections, incremental player numbering, SyncVars, and UI canvas instantiation without scene-switching overhead.

It serves as the gold-standard reference for direct, event-driven player spawning in Mirage.

![Basic Example](/img/examples/basic/Basic.PNG)

## How It Works

### `BasicPlayerSpawner`
`BasicPlayerSpawner` listens to events on `NetworkServer`:

- **`Server.Started`**: Resets the player counter so IDs start fresh for each server session.
- **`Server.Authenticated`**: Immediately marks `player.SceneIsReady = true` since single-scene games have no scene loading transition, then calls `SpawnCharacterForPlayer(player)`.
- **Pre-Spawn SyncVar Initialization**: Mutates `basicPlayer.playerNo` *before* calling `ServerObjectManager.AddCharacter()`. This ensures initial SyncVar values are serialized directly into the initial spawn message rather than requiring a second update packet.

```csharp
private void OnServerAuthenticated(INetworkPlayer player)
{
    // In a single-scene example without scene transitions, the client is immediately ready on authentication
    player.SceneIsReady = true;

    SpawnCharacterForPlayer(player);
}

private void SpawnCharacterForPlayer(INetworkPlayer player)
{
    Debug.Assert(player.Identity == null, "Player already has a character spawned.");

    // Instantiate under the UI parent canvas so the element appears in the layout hierarchy
    var character = Instantiate(PlayerPrefab, Parent);

    // Set initial SyncVars prior to AddCharacter so they are packed into the initial SpawnMessage
    var basicPlayer = character.GetComponent<BasicPlayer>();
    _playerCounter++;
    basicPlayer.playerNo = _playerCounter;

    // AddCharacter spawns the object on clients and assigns authority to the connection
    ServerObjectManager.AddCharacter(player, character.gameObject);
}
```

### `BasicPlayer`
The player UI card represents a connected player in the canvas hierarchy. It shows three ways of using SyncVars:

- **`[SyncVar(initialOnly = true)] playerNo`**: Set by `BasicPlayerSpawner` before the player is spawned and never changes, so `initialOnly` sends it once in the spawn message only.
- **`[SyncVar] playerColor`**: Set by the server in `OnStartServer`, which runs before the spawn message is sent, so clients already have it in `OnStartClient`. It is used as the color of the player's name text.
- **`[SyncVar(hook = nameof(OnPlayerDataChanged))] playerData`**: Changed by the server every second (`InvokeRepeating`). The hook runs on each client when the value changes, and updates the UI.

```csharp
[SyncVar(initialOnly = true)]
public int playerNo;

[SyncVar]
private Color playerColor;

[SyncVar(hook = nameof(OnPlayerDataChanged))]
public int playerData;

private void OnPlayerDataChanged(int oldPlayerData, int newPlayerData)
{
    playerDataText.text = string.Format("Data: {0:000}", newPlayerData);
}
```

- **Local player**: `OnStartLocalPlayer` only runs on the client that owns this player, and adds a shaded background so you can see which card is yours.

![Basic Player](/img/examples/basic/BasicPlayer.PNG)

## Running the Example

1. Import the **Basic** sample, then open `Scenes/Example.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD.
3. Launch a standalone client build (or another editor instance) and click **Client**.
4. Both instances will display active player cards in the UI canvas with live synchronized data.
