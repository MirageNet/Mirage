---
sidebar_position: 11
---

# Rigidbody Physics Example

The Rigidbody Physics example shows server-side physics synced to clients. Two balls bounce on a floor. Pressing **Space** on the server pushes them up, and every client sees them move.

<!-- TODO screenshot: the two balls mid-bounce with the labels above them -->

## How It Works

### Server-side physics
Each ball is a scene object with a `NetworkIdentity`, a `Rigidbody` and a `NetworkTransform`:

- The server simulates the physics, and the `NetworkTransform` (server authority) sends the ball's position and rotation to clients.
- Clients also have a non-kinematic `Rigidbody`, so they run their own physics between updates, and the `NetworkTransform` keeps pulling them back to the server's position.

:::tip
If you see the balls jitter on clients, set the `Rigidbody` to kinematic on clients (for example in `Identity.OnStartClient` when `!IsServer`), so only the server's position is used.
:::

### `AddForce`
`AddForce` only reacts to input on the server, so only the server can push the balls:

```csharp
private void Update()
{
    if (IsServer && Input.GetKeyDown(KeyCode.Space))
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * force);
    }
}
```

### Player
The scene uses [`NetworkSceneLoader`](/docs/guides/scene-loading/network-scene-loader) to spawn an empty player object for each client. The player doesn't do anything, but adding a character is what sends the existing scene objects (the balls) to that client.

## Running the Example

1. Import the **RigidbodyPhysics** sample, then open `Scenes/BounceScene.unity`.
2. Press **Play** in the Editor and click **Host (Server + Client)** in the HUD.
3. Start a built instance and click **Client**.
4. Press **Space** in the host window to push the balls up, and watch them move in the client too.
