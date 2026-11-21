# MIRAGE1001: SyncVar Class Warning

## When this appears

An ordinary class is used as a `[SyncVar]` value.

Generated readers allocate a new object for each non-null value received, adding garbage during frequent updates. Changing a member also bypasses SyncVar change tracking, so other peers can keep the old value.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-triggering' }}}

## How to fix

### Use a struct for small values

Use a struct for small values to avoid allocating the outer object. On the sending side configured by `SyncSettings`, copy the value, change the copy, and assign the whole value back.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-recommended' }}}

Changing a struct member in place still bypasses the setter. Struct members can allocate, and copies share reference members. Equality must distinguish the changes you want to synchronize.

### Keep a class

You can keep a serializable class. Assign a replacement value when it changes; reassigning the same mutated instance normally compares equal and does not mark a new update.

Custom `Write`/`Read` extension methods let you choose what is serialized. This example assigns new immutable values and supplies a custom serializer; its reader still allocates.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-custom' }}}

`[WeaverSafeClass]` on the class suppresses this warning wherever that class is used. To suppress it for one field, put the attribute on that field instead.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-suppress' }}}

The attribute changes neither serialization, equality, dirty tracking, nor hooks. It cannot make an unsupported type serializable.

## Details and exceptions

- Normal SyncVar assignments mark a change only when `EqualityComparer<T>.Default` finds a difference. Hook calls also depend on the network side and SyncVar options.
- With `initialOnly = true`, assignments only change local storage, without marking an update or invoking hooks. Only initial snapshots transmit the field; allocation and serialization behavior are unchanged.
- Generated serializers use eligible fields of the declared type and its base types. They do not automatically serialize properties or preserve derived runtime types. Custom readers can use different allocation behavior.
- `string` uses dedicated serialization. `NetworkIdentity`, `GameObject`, and `NetworkBehaviour` references identify existing spawned objects. These types are exempt; other `UnityEngine.Object` or component types are not automatically exempt.
