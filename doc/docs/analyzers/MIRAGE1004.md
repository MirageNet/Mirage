# MIRAGE1004: Reassignment of SyncObject Fields

## The Problem
A field implementing `ISyncObject` (such as `SyncList`, `SyncDictionary`, `SyncHashSet`, etc.) is reassigned, or is not marked `readonly`.

Fields implementing `ISyncObject` must be initialized once when the class is constructed and cannot be reassigned during the lifecycle of the object. Reassigning these fields breaks Mirage's post-processing code weaving and prevents proper dirty-tracking and delta serialization.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-triggering' }}}

---

## How to Resolve

Mark the field as `readonly` to ensure it cannot be reassigned. To clear or reset the collection, use the collection's `.Clear()` method instead of instantiating a new object.

{{{ Path:'Snippets/Analyzers/Mirage1004.cs' Name:'mirage1004-resolved' }}}
