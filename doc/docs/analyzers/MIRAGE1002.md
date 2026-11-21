# MIRAGE1002: Direct Mutation of SyncCollection Elements

## When this appears

A member of a `SyncList` or `SyncDictionary` element is changed without notifying the collection.

The collection records changes made through its own methods and indexer. It does not track fields inside an element, so other peers can keep the old value.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-triggering' }}}

## How to fix

Make changes on the sending side configured by `SyncSettings`.

### Struct values

Copy the element, change the copy, and assign it back through the indexer.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved' }}}

`SyncList` indexer assignments queue an update only when its comparer finds a difference. Equality that ignores the changed field, or a copied struct that still shares reference members, can hide a change.

`SyncDictionary` records existing-key assignments without comparing values.

### Class values in a SyncList

After mutation, call `SetItemDirtyAt(index)`. Reassigning the same mutated instance normally compares equal and queues nothing.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-class' }}}

### Class values in a SyncDictionary

Retrieve, mutate, and assign the value back to its key. This queues an update even for the same reference. `SetItemDirty` and `SetItemDirtyAt` belong to `SyncList`, not `SyncDictionary`.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-dictionary' }}}

## Details and exceptions

- For a struct element, C# rejects a direct field assignment such as `list[i].health = 10` with CS1612. Changing a local copy requires writing it back; reference members can still mutate shared state.
- `SetItemDirty(item)` finds a match using the list's comparer. Use `SetItemDirtyAt(index)` when position matters. Both invoke local `OnSet` with the current item as both old and new values, without a snapshot of its previous state.
- An initial snapshot or an already queued operation may include a nested change. That does not mean the collection tracks nested changes or will send later ones.
