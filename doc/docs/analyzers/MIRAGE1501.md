# MIRAGE1501: Network Message Serialized Size Estimation

## Description
Reports the estimated serialized size of `[NetworkMessage]` types to help optimize network bandwidth in the editor.

---

## Example
{{{ Path:'Snippets/Analyzers/Mirage1501.cs' Name:'mirage1501-example' }}}

---

## Size Estimation Details
* Primitives use standard serialization sizes.
* Unity structs (e.g., `Vector3`, `Quaternion`) use uncompressed precision unless packed.
* Dynamic types (strings, collections) add 1 byte for length prefix.
