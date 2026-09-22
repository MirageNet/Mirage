# MIRAGE1002: Direct Mutation of SyncCollection Elements

## When this appears

A `SyncList` or `SyncDictionary` element is mutated without notifying the collection. Initial snapshots or queued operations may include the mutation, but do not provide nested change tracking.

- **Classes:** `list[i].health = 10` changes the object without queuing an update.
- **Structs:** That direct field assignment is rejected by C# (CS1612). Changing a local copy requires write-back; reference members can still mutate shared state.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-triggering' }}}

## How to fix

Make changes on the sending side configured by `SyncSettings`.

### Struct values

Copy the element, modify it, and assign it back through the indexer. `SyncList` queues only values its configured comparer considers different; shared references or equality that ignores changed fields can defeat notification.

`SyncDictionary` records existing-key assignments without comparing values.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved' }}}

### Class values in a SyncList

After mutation, call `SetItemDirtyAt(index)`. Reassigning the same mutated instance normally compares equal and queues nothing.

`SetItemDirty(item)` locates a match using the list's comparer; use an index when position matters. Both dirty methods invoke local `OnSet` with the current item as both old and new values, without an old-state snapshot.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-class' }}}

### Class values in a SyncDictionary

Retrieve, mutate, and assign the value back to its key. This queues an update even for the same reference. `SetItemDirty` and `SetItemDirtyAt` belong to `SyncList`, not `SyncDictionary`.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-dictionary' }}}
