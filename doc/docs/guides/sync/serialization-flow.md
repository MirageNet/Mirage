---
sidebar_position: 4
---

## Serialization Flow

Game objects with the Network Identity component attached can have multiple scripts derived from [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour). The flow for serializing these game objects is:

On the server:
-   Each [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) has a dirty mask. This mask is available inside `OnSerialize` as `syncVarDirtyBits`
-   Each SyncVar in a [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) script is assigned a bit in the dirty mask.
-   Changing the value of SyncVars causes the bit for that SyncVar to be set in the dirty mask
-   Alternatively, calling `SetDirtyBit` writes directly to the dirty mask
-   [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity) game objects are checked on the server as part of its update loop
-   If any [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour)s on a [NetworkIdentity](/docs/reference/Mirage/NetworkIdentity) are dirty, then a `UpdateVars` packet is created for that game object
-   The `UpdateVars` packet is populated by calling `OnSerialize` on each [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) on the game object
-   [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour)s that are not dirty write a zero to the packet for their dirty bits
-   [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour)s that are dirty write their dirty mask, then the values for the SyncVars that have changed
-   If `OnSerialize` returns true for a [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour), the dirty mask is reset for that [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) so it does not send again until its value changes.
-   The `UpdateVars` packet is sent to ready clients that are observing the game object

On the client:
-   an `UpdateVars packet` is received for a game object
-   The `OnDeserialize` function is called for each [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) script on the game object
-   Each [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) script on the game object reads a dirty mask.
-   If the dirty mask for a [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) is zero, the `OnDeserialize` function returns without reading any more
-   If the dirty mask is a non-zero value, then the `OnDeserialize` function reads the values for the SyncVars that correspond to the dirty bits that are set
-   If there are SyncVar hook functions, those are invoked with the value read from the stream.
