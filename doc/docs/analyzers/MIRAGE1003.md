# MIRAGE1003: SyncObject fields must be marked as readonly

## The Problem
A `NetworkBehaviour` instance field whose type implements `Mirage.Collections.ISyncObject` (such as `SyncList`, `SyncDictionary`, or `SyncHashSet`) is not marked `readonly`, or its registered instance is replaced after construction.

Requiring `readonly` is an analyzer policy that protects the field's lifetime. The Weaver can register non-readonly fields; it does not enforce this modifier itself.

The Weaver adds constructor code that registers the field's current object with its `NetworkBehaviour`. Mirage retains that instance for change notifications and serialization. Replacing the field later leaves the original object registered and does not automatically register the replacement.

Initialize the field to a non-null instance in its initializer or during construction, before registration. Keep that reference for the behaviour's lifetime. `readonly` protects the reference; it does not make collection contents immutable. Ordinary `ISyncObject` use outside a `NetworkBehaviour` is not this auto-registration policy's scope.

SyncObjects provide their own synchronization and must not also be marked `[SyncVar]`; the Weaver rejects that combination. If a SyncObject field has `[SyncVar]`, remove the attribute and keep a stable `readonly` field. Do not remove `readonly` to apply the ordinary SyncVar remedy from [MIRAGE1005](./MIRAGE1005.md).

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-triggering' }}}

---

## How to Resolve

Mark the field `readonly` and initialize it once. On the sending side configured by `SyncSettings`, modify the existing collection through its APIs. For built-in collections, use `.Clear()` to remove the contents while preserving the registered instance. For custom `ISyncObject` types, use the appropriate operation on that instance; the interface does not require a `Clear()` method.

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-resolved' }}}
