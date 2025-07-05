# Chat Example

This example demonstrates a basic chat application using Mirage, showcasing how to send messages between clients via the server.

![Chat Example](/img/examples/chat/Chat.png)

## Setup

1.  Open the `Assets/Examples/Chat/Scenes/Main.unity` scene.
2.  Ensure this scene is added to your Build Settings (File > Build Settings).

## Key Components

### `ChatNetworkManager.cs`

This script extends `NetworkManager` and handles the custom player creation process. Instead of automatically spawning a player character, it registers a handler for a `CreateCharacterMessage`. When a client connects and authenticates, it sends this message to the server with the desired player name. The server then instantiates the `Player` prefab, assigns the provided name, and adds it as the player character.

### `ChatWindow.cs`

This script manages the chat UI. It displays incoming messages in a scrollable text area and allows the local player to send messages. It subscribes to the `Player.OnMessage` event to receive and display chat messages. When the send button is clicked, it calls a `[ServerRpc]` method on the local player to send the message to the server.

### `Player.cs`

This script is attached to the player prefab and represents a player in the chat. It contains:

-   A `SyncVar` for `playerName`, which is synchronized from the server to all clients.
-   A static `OnMessage` event that `ChatWindow` subscribes to, allowing messages to be displayed.
-   A `[ServerRpc]` method `CmdSend` that clients call to send a message to the server.
-   A `[ClientRpc]` method `RpcReceive` that the server calls on all clients to broadcast the message.

## How to Run

1.  **Run as Host:** Press Play in the Unity Editor. The game will start as a host (server + client).
2.  **Run as Client:** Build and run a standalone instance of the game. In the standalone instance, click the "Client" button to connect to the host running in the editor.
3.  Type messages into the input field and press Enter (or click Send). Observe how messages are sent between the host and client, with different colors indicating local and remote messages.

This example demonstrates a simple yet effective way to implement real-time communication between networked clients in Mirage.