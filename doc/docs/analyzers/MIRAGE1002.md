# MIRAGE1002: Direct Mutation of SyncCollection Elements

## The Problem
A member or element within a `SyncList` or `SyncDictionary` is mutated directly without assigning it back to the collection or calling a change notification method.

Direct modifications like `mySyncList[i].health = 10;` will not synchronize to clients due to how C# handles collections and types:
- For Structs (Value Types): You are only modifying a temporary local copy, not the item inside the collection.
- For Classes (Reference Types): While the object itself is mutated, this action bypasses the collection's indexer setter.

Because the collection cannot detect these nested field changes, it never sets the dirty flag, preventing the update from syncing.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-triggering' }}}

---

## How to Resolve

Depending on whether your collection stores structs (value types) or classes (reference types), resolve this in one of two ways:

### Solution 1: For Structs (Value Types)
Retrieve the element, modify it, and assign it back to the collection using the indexer. This triggers the collection's indexer setter.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved' }}}

### Solution 2: For Classes (Reference Types)
Directly mutate the fields of the object inside the collection, and then manually mark the item as dirty using `SetItemDirty` or `SetItemDirtyAt` so that the change is serialized and synchronized.

{{{ Path:'Snippets/Analyzers/Mirage1002.cs' Name:'mirage1002-resolved-class' }}}

