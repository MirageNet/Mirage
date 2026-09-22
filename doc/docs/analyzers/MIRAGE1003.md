# MIRAGE1003: SyncObject fields must be marked as readonly

## When this appears

A `NetworkBehaviour` instance field implementing `Mirage.Collections.ISyncObject`, such as `SyncList`, `SyncDictionary`, or `SyncHashSet`, lacks `readonly` or is replaced after construction.

The Weaver registers the field's object during construction. Replacing it leaves the original object registered; the replacement is not registered automatically.

Requiring `readonly` protects that lifetime. It is an analyzer policy, not a modifier enforced by the Weaver.

Ordinary `ISyncObject` use outside a `NetworkBehaviour` is outside this policy.

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-triggering' }}}

## How to fix

- Initialize a non-null instance in the field initializer or constructor, before registration.
- Mark the field `readonly` and keep that instance for the behaviour's lifetime. Its contents remain mutable.
- On the sending side configured by `SyncSettings`, change contents through the object's APIs. Built-in collections support `Clear()`; custom `ISyncObject` types need their own appropriate operation.

SyncObjects synchronize themselves and must not also have `[SyncVar]`; the Weaver rejects that combination. Remove `[SyncVar]` and keep `readonly`, instead of applying the ordinary SyncVar fix from [MIRAGE1005](./MIRAGE1005.md).

{{{ Path:'Snippets/Analyzers/Mirage1003.cs' Name:'mirage1003-resolved' }}}
