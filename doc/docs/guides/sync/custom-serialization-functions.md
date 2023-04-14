---
sidebar_position: 7
---

# Advanced State Synchronization

In most cases, the use of SyncVars is enough for your game scripts to serialize their state to clients. However, in some cases, you might require more complex serialization code. This page is only relevant for advanced developers who need customized synchronization solutions that go beyond Mirage’s normal SyncVar feature.

## Custom Serialization Functions

To perform your own custom serialization, you can implement virtual functions on [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) to be used for SyncVar serialization. These functions are:

```cs
public virtual bool OnSerialize(NetworkWriter writer, bool initialState);
```

```cs
public virtual void OnDeserialize(NetworkReader reader, bool initialState);
```

Use the `initialState` flag to differentiate between the first time a game object is serialized and when incremental updates can be sent. The first time a game object is sent to a client, it must include a full state snapshot, but subsequent updates can save on bandwidth by including only incremental changes.


The `OnSerialize` function should return true to indicate that an update should be sent. If it returns true, the dirty bits for that script are set to zero. If it returns false, the dirty bits are not changed. This allows multiple changes to a script to be accumulated over time and sent when the system is ready, instead of every frame.

The `OnSerialize` function is only called for `initialState` or when the [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) is dirty. A [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) will only be dirty if a `SyncVar` or `SyncObject` (e.g. `SyncList`) has changed since the last OnSerialize call. After data has been sent the [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) will not be dirty again until the next `syncInterval` (set in the inspector). A [NetworkBehaviour](/docs/reference/Mirage/NetworkBehaviour) can also be marked as dirty by manually calling `SetDirtyBit` (this does not bypass the `syncInterval` limit).
 
Although this works,  it is usually better to let Mirage generate these methods and provide [custom serializers](/docs/guides/serialization/data-types) for your specific field.
