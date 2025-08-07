---
sidebar_position: 5
---
# Sync Settings

The `Sync Settings` can be found in each `NetworkBehaviour` component. It is only visible in the inspector when there are either `SyncVar`, `SyncObject`, or if `OnSerialize` is overridden. These settings allow you to configure how and when data is synchronized across the network. 

Using the `Sync Settings`, you can set the direction values are synced and how often they are sent.


## Sync Direction

SyncDirection determines which directions changes are sent. The default sync direction is from `Server` to both `Owner` and `Observers`. 

The directions can be set per NetworkBehaviour, which means that different components can have different sync directions. For example, a PlayerName component can be set to sync from the `Owner` and a Health component from the `Server`.

Valid sync directions include:

- Sync from `Server` to `Owner` and/or `Observers` (Default)
- Sync from `Owner` to `Server` and/or `Observers`
- Sync from both `Owner` and `Server` to `Observers` only
- No sync direction (None to None), nothing will be synced.

Invalid sync directions include:
- Sync from `None` to any direction
- Sync from `Owner` to None or `Server` to None
- Sync from `Server` to Owner only
- Sync from both Owner and `Server` to Owner only

:::note
`ObserversOnly` excludes the `Owner`. In order to sync to owner the `Owner` Flag must be set.
:::

If syncing from both `Owner` and `Server` at the same time, there will be a race condition. If they both update a value at the same time, they will both send an update to the other side which will set the value and cause them to be out of sync. While this option is allowed, it is advised to only sync from either `Server` or `Owner`.


### When to use Server to Owner

In some case you don't want some data to be visible to other players. By disabling the `ObserversOnly` flag Mirage will only send data to the Player that owns the object.

For example, suppose you are making an inventory system. Suppose players A, B, and C are in the same area. There will be a total of 12 objects in the entire network, objects in bold are owned by that client:

- Client A has **Player A**, Player B, and Player C
- Client B has Player A, **Player B**, and Player C
- Client C has Player A, Player B, and **Player C**
- The server has Player A, Player B, Player C

each one of them would have an Inventory component

Suppose Player A picks up some loot. The server adds the loot to Player's A inventory, which could have a [SyncLists](/docs/guides/sync/sync-objects/sync-list) of Items. 

By default, Mirage now has to synchronize player A's inventory everywhere, which means sending an update message to client A, client B, and client C, because they all have a copy of Player A. This is wasteful, Client B and Client C do not need to know about Player's A inventory, they never see it on screen. It is also a security problem, someone could hack the client and display other people's inventory and use it to their advantage.

By only having `SyncTo.Owner` set the server will only send then Player A's to Client A, Player B's to Client B, etc.  

It might not seem like much of a waste with only 3 players, but say if you have 50 instead the that is a lot of extra data to sending to each client.

Other typical use cases include quests, player's hand in a card game, skills, experience, or any other data you don't need to share with other players.


## Sync Timing

The `SyncTiming` determines how the minium time between changes being sent. Values are not sent unless they are changed or manually set as dirty.

The `SyncTiming` enum has the following values:

- `Variable`
- `Fixed`
- `NoInterval`

`Variable` and `Fixed` will use the Interval field to determine how often changes are sent. `NoInterval` will sent changes next time Update is run

### Variable

The `Variable` timing mode waits for at least the specified `Interval` time after the last sync before sending again. This timing mode is best used when values don't change often or for non-time-critical data. Compared to the `Fixed` timing mode, the `Variable` timing mode sends data less often for the same `Interval`. 

### Fixed

The `Fixed` timing mode ensures that data is sent every `Interval` if changed. This timing mode is best used for data that changes often and you want exactly `(1 / Interval)` updates per second. The `Fixed` timing mode has a more consistent sync time compared to the `Variable` timing mode. 

#### Example of Fixed vs Variable

For example, if `Interval` is 0.1, the `Fixed` timing mode will send data at a constant rate of 10 times per second, while the `Variable` timing mode will depend more on the deltaTime and may send data at irregular intervals. This means that `Variable` mode will send less often/

### NoInterval

The `NoInterval` timing mode ignores `SyncSettings.Interval` and sends changes in the next update. This timing mode is best used for scenarios where data changes frequently and sending updates as soon as possible is desired.


## When are changes sent?

When a value is changed, the corresponding `NetworkBehaviour` is added to a list of dirty objects. This list is then looped over during each update, and changes are sent while taking the `SyncInterval` into account.

This means that the server only has to loop over a small number of objects each frame - only the objects that have changed. This is a significant improvement in performance, especially when there are a large number of idle network objects.

In contrast, other networking solutions like Mirror Networking loop over every object every update, regardless of whether it has changed or not.

Because of this improvement, certain tasks can be accomplished more easily in Mirage. For example, if you have a forest with individual trees represented by NetworkIdentity, they will not add any performance cost unless the player interacts with them. This allows you to use RPCs and SyncVars on each tree, making it easier to create network features without needing workarounds for performance reasons.
