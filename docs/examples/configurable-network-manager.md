
# Configurable NetworkManager Example

This example demonstrates how to create a custom `NetworkManager` that allows for easy switching between different transport layers (e.g., UDP, Steam, or an offline mode) before the network is started. This is a common requirement for games that offer multiple ways to connect.

## Setup

1. Create a new GameObject and add the `ConfigurableNetworkManager` component to it.
2. In the inspector, assign your `UdpTransport` and `SteamTransport` socket factory assets to the corresponding fields.

## Key Components

### `ConfigurableNetworkManager.cs`

This script extends `NetworkManager` and provides a way to configure the transport layer before starting the server or client. It includes:

-   **Public Methods:** `StartHost`, `StartClient`, and `StartServer` methods that take a `SocketType` enum as an argument. These methods can be easily hooked up to a UI to allow the player to choose their desired connection method.
-   **Transport Configuration:** The `ConfigureNetwork` method is called before starting the network to set the `SocketFactory` and `PeerConfig` on the `NetworkServer` and `NetworkClient`. This is where the core logic for switching transports resides.
-   **Offline Mode:** The example shows how to implement an offline mode by setting `Server.Listening` to `false`. This prevents the server from listening for incoming connections, effectively running it in a single-player mode.

### `SocketType` Enum

This simple enum defines the available transport options. In this example, it includes `Offline`, `UDP`, and `Steam`.

### PeerConfig

The `PeerConfig` object allows you to fine-tune the transport layer's behavior. Setting this is optional, but the default values might be too low for some projects. You can find more details about the available settings in the API reference for the [Config](/docs/reference/Mirage.SocketLayer/Config) class.

## How to Use

1.  **Create a UI:** In your scene, create a UI with buttons that call the public methods on the `ConfigurableNetworkManager` script. For example, you could have a "Start Host (UDP)" button that calls `StartHost(SocketType.UDP)`.
2.  **Assign Transports:** In the inspector for your `ConfigurableNetworkManager` GameObject, assign your `UdpTransport` and `SteamTransport` assets to the corresponding fields.
3.  **Run the Game:** When you run the game, you can use your UI to start the server, client, or host with the desired transport layer.

This example provides a clean and reusable way to manage multiple transport layers in your game, giving you the flexibility to support different networking backends.
