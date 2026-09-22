# MIRAGE1003: SyncObject fields must be marked as readonly

## When this appears

A SyncObject instance field in a `NetworkBehaviour` is missing `readonly` or is replaced after construction.

Weaver registers the original object during construction. A replacement is not registered automatically, so its changes can stay local while the old object continues to synchronize.

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-triggering' }}}

## How to fix

- Initialize a non-null instance in the field initializer or constructor, before registration.
- Mark the field `readonly` and keep that instance for the behaviour's lifetime. You can still change its contents.
- On the sending side configured by `SyncSettings`, change contents through the object's APIs. Use `Clear()` to empty a built-in collection; custom `ISyncObject` types need their own operation for changing or clearing contents.

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-resolved' }}}

SyncObjects synchronize themselves and must not also have `[SyncVar]`; Weaver rejects that combination. Remove `[SyncVar]` and keep `readonly`, instead of applying the ordinary SyncVar fix from [MIRAGE1005](./MIRAGE1005.md).

This rule covers instance fields implementing `Mirage.Collections.ISyncObject`, including `SyncList`, `SyncDictionary`, and `SyncHashSet`. Ordinary `ISyncObject` use outside a `NetworkBehaviour` is not covered.

The analyzer requires `readonly` to prevent replacement. Weaver can register a field without it, but does not protect that field from later replacement.
