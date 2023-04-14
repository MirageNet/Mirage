---
sidebar_position: 1
---
# State Synchronization

State synchronization refers to the synchronization of values such as integers, floating point numbers, strings, and boolean values belonging to scripts.

State synchronization is done from the server to remote clients. The local client does not have data serialized to it. It does not need it, because it shares the scene with the server. However, `SyncVar` hooks are called on local clients.

Data is not synchronized in the opposite direction - from remote clients to the server. To do this, you need to use Server RPC calls.
-   [SyncVar](/docs/guides/sync/sync-var)  
    SyncVars are variables of scripts that inherit from [`NetworkBehaviour`](/docs/reference/Mirage/NetworkBehaviour), which are synchronized from the server to clients. 
-   [SyncList](/docs/guides/sync/sync-objects/sync-list)  
    SyncLists contain lists of values and synchronize data from servers to clients.
-   [SyncDictionary](/docs/guides/sync/sync-objects/sync-dictionary)  
    A SyncDictionary is an associative array containing an unordered list of key, value pairs.
-   [SyncHashSet](/docs/guides/sync/sync-objects/sync-hash-set)  
    An unordered set of values that do not repeat.
-   [SyncSortedSet](/docs/guides/sync/sync-objects/sync-sorted-set)  
    A sorted set of values that do not repeat.
-   [SyncPrefab](/docs/guides/sync/sync-prefab)  
    Wrapper around `NetworkIdentity` to allow a prefab to be synced
