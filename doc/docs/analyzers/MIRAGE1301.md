# MIRAGE1301: Field Type Serialization Validation

## The Problem
A field or property in a class/struct marked with `[NetworkMessage]`, or a parameter in a method marked with `[ServerRpc]` or `[ClientRpc]`, uses a type that Mirage does not know how to serialize, and no custom writer/reader has been registered or generated for it.

Mirage uses compile-time IL weaving to generate serialization code for NetworkMessages and RPCs. If a field or parameter type is not a primitive type, an existing supported type, or a type that can be auto-weaved (like a simple struct/class with only serializable fields), and there are no custom `NetworkWriter` or `NetworkReader` extension methods for it, the Weaver will fail because it cannot serialize the data.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-triggering' }}}

---

## How to Resolve

Ensure all fields are of serializable types. If you need to send a custom type, make sure it is a struct/class with only serializable fields, or implement custom `Write` and `Read` extension methods for the custom type so that Mirage knows how to serialize it.

{{{ Path:'Snippets/Analyzers/Mirage1301.cs' Name:'mirage1301-resolved' }}}
