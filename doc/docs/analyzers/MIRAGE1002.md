# MIRAGE1002: Direct Mutation of SyncCollection Elements

## The Problem
A member of a value stored in a `SyncList` or `SyncDictionary` is changed without recording an update through the collection's API.

The collection records operations such as additions and indexer assignments; it does not observe arbitrary nested mutations.

- **Classes:** `mySyncList[i].health = 10` changes the object but bypasses the collection's setter and does not queue a new update.
- **Structs:** Assigning a field directly through these non-ref indexers, such as `mySyncList[i].health = 10`, is rejected by C# (CS1612). Modifying a local struct copy without assigning it back changes only that copy. Reference members inside a struct can still be mutated through shared references.

An initial snapshot or an already queued operation can happen to include later class mutations. That does not make nested mutation reliable change tracking.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-triggering' }}}

---

## How to Resolve

Make changes on the sending side configured by `SyncSettings`. Use the operation appropriate to the collection and its value type.

### Solution 1: For Structs (Value Types)
Retrieve the element, modify it, and assign it back using the indexer. A `SyncList` queues the assignment only if its configured equality comparer considers the old and new values different. Ensure equality reflects the changed data; a shallow copy containing shared mutable references may still compare equal. A `SyncDictionary` records an assignment to an existing key without this value-equality check.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved' }}}

### Solution 2: For Classes in a SyncList
After mutating an object, call `SetItemDirtyAt(index)` to queue that element. `SetItemDirty(item)` locates an item using the list's equality comparer; use an index when the intended position matters. Assigning the same mutated class instance back through the list indexer normally compares equal and queues nothing.

Manual dirty notification invokes the local `OnSet` callback with the current item for both old and new values; it does not preserve a snapshot of the old object.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-class' }}}

### Solution 3: For Classes in a SyncDictionary
Retrieve the value, mutate it, and assign it back to its key. The dictionary records this assignment even when it is the same reference. `SetItemDirty` and `SetItemDirtyAt` are `SyncList` APIs, not `SyncDictionary` APIs.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-dictionary' }}}
