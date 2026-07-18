# MIRAGE1201: NetworkMessage/RPC Class Warning

## The Problem
A field or property inside a `[NetworkMessage]`, or a parameter/return type in a `[ServerRpc]` or `[ClientRpc]` method is declared using a class type instead of a value type/struct.

Class-based types are generally risky for network serialization because:
1. **Allocations:** Mirage must allocate a new object instance upon deserialization, which causes garbage collection (GC) spikes and performance overhead.
2. **Polymorphism Limitations:** Mirage's standard serialization only serializes fields of the declared type, not the concrete derived subclass type.

*Note: Standard collections like `List<T>` or `Dictionary<K, V>` are natively supported and ignored.*

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-triggering' }}}

---

## How to Resolve

### Recommended Fix: Use a struct
Structs (value types) avoid memory allocation, support standard change tracking, and guarantee safe value-copy semantics.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-recommended' }}}

---

### Alternative Solutions
If a struct is not viable for your use case (e.g., you require complex inheritance), you can:

#### 1. Implement Custom Serialization
If you want to use the class type and manage performance/reference safety yourself, write custom `Write` and `Read` extension methods for the class, and decorate the class with `[WeaverSafeClass]` to suppress the warning globally.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-custom' }}}

#### 2. Suppress the warning on the field
If you want to disable the warning only on a specific field, property, or parameter, decorate it with `[WeaverSafeClass]`.
{{{ Path:'Snippets/Analyzers/Mirage1201.cs' Name:'mirage1201-alternative-suppress' }}}
