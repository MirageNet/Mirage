# MIRAGE1001: SyncVar Class Warning

## When this appears

An ordinary class is used as a `[SyncVar]` payload. This warning concerns allocation and updates; many class payloads are serializable.

Exceptions: `string` uses dedicated serialization; `NetworkIdentity`, `GameObject`, and `NetworkBehaviour` references identify existing spawned objects. Other `UnityEngine.Object` or component types are not automatically exempt.

- **Allocation:** Generated reads allocate non-null class payloads; custom readers may differ.
- **Representation:** Generated serializers use eligible fields of the declared type and its bases, without automatically serializing properties or preserving derived runtime types.
- **Updates:** Nested mutations bypass the setter. Default SyncVar assignments dirty only when `EqualityComparer<T>.Default` detects a difference. Reassigning the same mutated instance normally does nothing. Hooks depend on network side and options.

With `initialOnly = true`, assignments change local storage without dirtying or invoking hooks. Only initial snapshots transmit the field; the allocation and representation rules still apply.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-triggering' }}}

## How to fix

### Use a struct for small values

On the sending side configured by `SyncSettings`, copy, modify, and assign the whole value back. In-place member writes still bypass the setter.

Structs avoid the outer object allocation. Members can still allocate, copies share reference members, and equality must distinguish the synchronized changes.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-recommended' }}}

### Keep a class

Custom `Write`/`Read` extension methods control representation. This example replaces immutable values; its reader still allocates.

`[WeaverSafeClass]` on the payload type suppresses this warning for its uses. It changes neither serialization, equality, dirty tracking, nor hooks, and cannot make an unsupported type serializable.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-custom' }}}

For one field, place `[WeaverSafeClass]` on that field instead.

{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-suppress' }}}
