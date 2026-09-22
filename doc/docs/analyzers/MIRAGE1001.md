# MIRAGE1001: SyncVar Class Warning

## The Problem
An ordinary class payload is used as the value of a `[SyncVar]` field. Mirage can serialize many such classes; this warning is an analyzer policy about their allocation and update semantics, not a ban on reference types.

With generated serialization and change tracking:

1. **Allocations:** Reading a non-null ordinary class payload creates a new instance. Custom readers can use a different allocation strategy.
2. **Representation:** Generated serializers use eligible fields of the declared type and its base types. They do not automatically serialize properties or preserve the concrete derived type of a polymorphic value.
3. **Nested mutations:** Changing a member or a nested collection does not assign the SyncVar itself, so it bypasses the generated setter. Assignments are compared with `EqualityComparer<T>.Default`; a value that compares equal does not set a dirty bit. Assigning the same mutated instance back is therefore normally insufficient. Hook execution also depends on the SyncVar's hook options and the network side.

Supported values such as `string`, `NetworkIdentity`, `GameObject`, and `NetworkBehaviour` references are exceptions to this class-payload warning. Strings have dedicated serialization; network object references identify existing spawned objects rather than copying their fields. This is not a general exemption for every `UnityEngine.Object` or component type.

For `[SyncVar(initialOnly = true)]`, assignments update the local field but do not mark it dirty or invoke hooks. Only initial snapshots transmit it; copy–modify–assign does not enable later incremental updates. The class allocation and representation considerations still apply.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a struct
Use a struct for a small data value, then copy it, modify the copy, and assign the complete value back to the SyncVar on its configured sending side. Changing `data.health` in place still bypasses the SyncVar setter.

Structs avoid allocating the outer payload object, but their members can still allocate. Copies are shallow: mutable reference members remain shared, and equality must distinguish the synchronized values you intend to send.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-recommended' }}}

---

### Alternative Solutions
If a class is appropriate for your data model, choose its serialization and replacement behavior deliberately.

#### 1. Implement Custom Serialization
Custom `Write` and `Read` extension methods control the wire representation. The following example uses immutable class values and replaces the SyncVar when updating it. Its reader still allocates a new instance; custom serialization does not itself add nested mutation tracking or eliminate allocations.

`[WeaverSafeClass]` on the payload type acknowledges this policy warning for uses of that type. The marker does not change serialization, equality, dirty tracking, or hooks.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-custom' }}}

#### 2. Suppress the warning on the field
To acknowledge the warning only for a particular field, decorate that field with `[WeaverSafeClass]`. This suppresses the warning; it does not fix nested mutations or make an unsupported type serializable.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-alternative-suppress' }}}
