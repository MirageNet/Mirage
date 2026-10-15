---
sidebar_position: 9
---
# Network Transform

:::danger
NetworkTransform is not optimized and should not be used in production. It is recommended to use [NetworkPositionSync](https://github.com/James-Frowen/NetworkPositionSync) instead.
:::

The Network Transform component synchronizes the position, rotation, and scale of networked game objects across the network.

A game object with a Network Transform component must also have a Network Identity component. When you add a Network Transform component to a game object, Mirage also adds a Network Identity component on that game object if it does not already have one.

![The Network Transform component](/img/components/NetworkTransform.png)

By default, Network Transform is server-authoritative unless you check the box for **Client Authority**. Client Authority applies to character objects as well as non-character objects that have been specifically assigned to a client but only for this component. With this enabled, position changes are sent from the client to the server.

Under **Sensitivity**, you can set the minimum thresholds of change to the transform values in order for network messages to be generated. This helps minimize network "noise" for minor twitch and jitter.

**Sync Mode** and **Sync Interval** are controlled by the `NetworkBehaviour`'s `SyncSettings`.

