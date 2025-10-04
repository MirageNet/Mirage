---
sidebar_position: 2
title: Network Manager
---
# Network Manager Callbacks

**See also [NetworkManager](/docs/reference/Mirage/NetworkManager) in the API Reference.**

`NetworkManager` is a helper class with instance references to the other major parts of the Mirage network. It acts as the central hub for managing networking aspects of your game, providing easy access to core components and handling various network events.

## Key Properties

-   **`Server`**: Reference to the `NetworkServer` instance, used for server-side operations.
-   **`Client`**: Reference to the `NetworkClient` instance, used for client-side operations.
-   **`NetworkSceneManager`**: Manages scene loading and unloading across the network.
-   **`ServerObjectManager`**: Manages spawning and despawning of networked objects on the server.
-   **`ClientObjectManager`**: Manages spawning and despawning of networked objects on the client.
-   **`IsNetworkActive`**: Returns `true` if either the server or client is active and running.
-   **`NetworkMode`**: An enum (`NetworkManagerMode`) indicating the current network mode (None, Server, Client, Host).

## Important Events

`NetworkManager` exposes several events that you can subscribe to for various network lifecycle changes:

-   **`Server.Started`**: Invoked when the server starts.
-   **`Server.Stopped`**: Invoked when the server stops.
-   **`Client.Connected`**: Invoked when the client successfully connects to a server.
-   **`Client.Disconnected`**: Invoked when the client disconnects from a server.

## Usage

Typically, you will have a single `NetworkManager` GameObject in your scene. It automatically sets up references to other necessary Mirage components if `ValidateReferences` is enabled. You can then use its properties and subscribe to its events to control your game's networking logic.

Example of subscribing to an event:

```csharp
public class MyGameManager : MonoBehaviour
{
    public NetworkManager networkManager;

    void Start()
    {
        networkManager.Client.Connected.AddListener(OnClientConnected);
    }

    void OnClientConnected(INetworkPlayer player)
    {
        Debug.Log("Client connected to server!");
    }
}
```