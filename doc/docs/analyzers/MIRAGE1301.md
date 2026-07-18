# MIRAGE1301: Field Type Serialization Validation

## The Problem
A network message field, RPC parameter, or property uses a type that Mirage cannot serialize, and no custom writer or reader has been registered for it.

Mirage uses compile-time weaving to generate serialization code. If a type is not natively supported, auto-weavable, or registered with custom `NetworkWriter`/`NetworkReader` extension methods, the weaver will fail.

Types can be auto-weaved if they are simple structs or non-generic classes with a parameterless constructor and only serializable fields.

Internal fields and fields marked `[NonSerialized]` are ignored during serialization and will not cause errors.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-triggering' }}}

---

## How to Resolve

Make sure all fields and properties use serializable types. Custom classes must be non-generic, have a parameterless constructor, and contain only serializable fields. Alternatively, implement custom `Write` and `Read` extension methods for the type.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-resolved' }}}
