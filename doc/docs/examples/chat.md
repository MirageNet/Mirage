---
sidebar_position: 5
---

# Chat Example

The Chat example demonstrates how to use custom network messages for player character setup and Remote Procedure Calls (RPCs) to broadcast real-time text chat across multiple clients.

![Chat Example](/img/examples/chat/Chat.png)

## How It Works

### `ChatNetworkManager`
`ChatNetworkManager` extends `NetworkManager` to manage client connection lifecycle and character spawning via custom messages:

- **Custom Character Request Message**: Defines `CreateCharacterMessage` containing the client's chosen display name.
- **Client Lifecycle**:
  - `Client.Authenticated`: Activates the `ChatWindow` UI on the client and sends `CreateCharacterMessage` to the server.
  - `Client.Disconnected`: Deactivates the `ChatWindow` UI when the connection closes.
- **Server Character Creation**: `OnServerStarted` registers a handler for `CreateCharacterMessage`. When received, the server instantiates the player prefab, assigns `playerName`, and spawns it using `ServerObjectManager.AddCharacter(player, playergo)`.

```csharp
[NetworkMessage]
public struct CreateCharacterMessage
{
    public string name;
}

private void OnClientAuthenticated(INetworkPlayer player)
{
    // Open the chat window on the client once authenticated
    chatWindow.gameObject.SetActive(true);

    // Request the server to create a player character with our chosen name
    player.Send(new CreateCharacterMessage { name = PlayerName });
}

private void OnClientDisconnected(ClientStoppedReason reason)
{
    // Hide the chat window when client disconnects
    chatWindow.gameObject.SetActive(false);
}

private void OnCreatePlayer(INetworkPlayer player, CreateCharacterMessage createCharacterMessage)
{
    // Create a gameobject using the name supplied by client
    var character = Instantiate(playerPrefab);
    character.playerName = createCharacterMessage.name;

    // Spawn the character and assign authority to the player connection
    ServerObjectManager.AddCharacter(player, character.gameObject);
}
```

### `Player` & RPC Broadcasting
The `Player` script handles sending and receiving chat messages:

- **`[SyncVar] public string playerName`**: Synchronizes the player's name across all clients.
- **`[ServerRpc] public void CmdSend(string message)`**: Called by the local client to send a chat message to the server.
- **`[ClientRpc] public void RpcReceive(string message)`**: Called by the server to send the message to every client. It raises the static `Player.OnMessage` event, so the networking code doesn't need to know about the UI.

```csharp
public static event Action<Player, string> OnMessage;

[ServerRpc]
public void CmdSend(string message)
{
    if (message.Trim() != "")
        RpcReceive(message.Trim());
}

[ClientRpc]
public void RpcReceive(string message)
{
    OnMessage?.Invoke(this, message);
}
```

### `ChatWindow`
`ChatWindow` is the UI. It subscribes to `Player.OnMessage` in `Awake`, and colors the name red for your own messages and blue for other players:

```csharp
private void OnPlayerMessage(Player player, string message)
{
    var prettyMessage = player.IsLocalPlayer ?
        $"<color=red>{player.playerName}: </color> {message}" :
        $"<color=blue>{player.playerName}: </color> {message}";
    AppendMessage(prettyMessage);
}
```

When you click **Send**, it finds the local player's `Player` component (`Client.Player.Identity`) and calls `CmdSend`.

## Running the Example

1. Import the **Chat** sample, then open `Scenes/Main.unity`.
2. Press **Play** in the Editor, type a name in the name field, and click **Host (Server + Client)** in the HUD.
3. Start a built instance, enter a different name, and click **Client**.
4. Type messages in the chat input field and click **Send** to chat across instances.
