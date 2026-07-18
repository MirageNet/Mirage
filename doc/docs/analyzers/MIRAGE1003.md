# MIRAGE1003: SyncObject fields must be marked as readonly

## The Problem
A field implementing `ISyncObject` (such as `SyncList`, `SyncDictionary`, `SyncHashSet`, etc.) is reassigned or is not marked as `readonly`.

These fields must be initialized once during construction and remain read-only throughout the lifecycle of the object. Reassignment breaks Mirage's post-processing code weaving, which prevents proper dirty-tracking and delta serialization.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-triggering' }}}

---

## How to Resolve

Mark the field as `readonly` to ensure it cannot be reassigned. To clear or reset the collection, use the collection's `.Clear()` method instead of instantiating a new object.

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-resolved' }}}
