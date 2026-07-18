# MIRAGE1201: NetworkMessage/RPC Class Warning

## The Problem
A field or property inside a `[NetworkMessage]`, or a parameter/return type in a `[ServerRpc]` or `[ClientRpc]` method uses a class instead of a struct.

Using class types is risky because:
- **Allocations:** Mirage must allocate a new instance on deserialization, causing garbage collection spikes.
- **No Polymorphism:** Only fields of the declared type are serialized, not the derived class type.

*Note: Standard collections like `List<T>` or `Dictionary<K, V>` are natively supported and ignored.*

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a struct
Structs avoid allocations and guarantee copy-by-value semantics.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-recommended' }}}

---

### Alternative Solutions
If a struct is not viable (e.g. for complex inheritance):

#### Custom Serialization
Write custom `Write`/`Read` extension methods and decorate the class with `[WeaverSafeClass]` to suppress the warning globally.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-custom' }}}

#### Local Suppression
Decorate the specific field, property, or parameter with `[WeaverSafeClass]` to suppress the warning locally.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-suppress' }}}
