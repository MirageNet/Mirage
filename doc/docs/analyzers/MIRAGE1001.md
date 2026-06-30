# MIRAGE1001: SyncVar Class Warning

## The Problem
A field marked with `[SyncVar]` is declared using a class type instead of a value type/struct.

Class-based types are generally risky for network synchronization because:
1. **Allocations:** Mirage must allocate a new object instance upon deserialization, which causes garbage collection (GC) spikes and performance overhead.
2. **Polymorphism Limitations:** Mirage's standard serialization only serializes fields of the declared field type, not the concrete derived subclass type.
3. **Change Tracking Gotchas:** If you modify an internal field of the class instance on the server, its object reference stays the same. Because the reference hasn't changed, Mirage cannot automatically detect the modification to set the dirty flag and fire sync hooks.
4. **Collection Syncing:** Modifying contents of lists/dictionaries inside a SyncVar will not trigger a synchronization because the container reference does not change.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-triggering' }}}

---

## How to Resolve

### Option 1: Use a struct (Recommended)
Structs (value types) avoid memory allocation, support standard change tracking, and guarantee safe value-copy semantics.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-struct-option' }}}

### Option 2: Implement Custom Serialization and mark the class as safe
If you want to use the class type and manage performance/reference safety yourself, write custom `Write` and `Read` extension methods for the class, and decorate the class with `[WeaverSafeClass]` to suppress the warning globally.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-class-option' }}}

### Option 3: Suppress the warning on the field
If you want to disable the warning only on a specific field, decorate the field with `[WeaverSafeClass]`.
{{{ Path:'Snippets/Analyzers/Mirage1001.cs' Name:'mirage1001-suppress-option' }}}
