---
sidebar_position: 10
---
# Network Transform Child

:::danger
NetworkTransformChild is not optimized and should not be used in production. It is recommended to use [NetworkPositionSync](https://github.com/James-Frowen/NetworkPositionSync) instead.
:::

The Network Transform Child component synchronizes the position and rotation of the child game object of a game object with a Network Transform component. You should use this component in situations where you need to synchronize an independently-moving child object with a Networked game object.

To use the Network Transform Child component, attach it to the same parent game object as the Network Transform, and use the Target field to define which child game object to apply the component settings to. You can have multiple Network Transform Child components on one parent game object.

![The Network Transform component](/img/components/NetworkTransform.png)

Under **Sensitivity**, you can set the minimum thresholds of change to the transform values in order for network messages to be generated. This helps minimize network "noise" for minor twitch and jitter.

**Sync Mode** and **Sync Interval** are controlled by the `NetworkBehaviour`'s `SyncSettings`.

This component takes authority into account, so local player game objects (which have local authority) synchronize their position from the client to the server, then out to other clients. Other game objects (with server authority) synchronize their position from the server to clients.
