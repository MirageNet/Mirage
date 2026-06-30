# MIRAGE1501: Network Message Serialized Size Estimation

## Description
This diagnostic runs on all `[NetworkMessage]` types to report their estimated serialized size. This helps developers analyze and optimize the bandwidth footprint of their network messages directly in the editor.

---

## Example
{{{ Path:'Snippets/Analyzers/Mirage1501.cs' Name:'mirage1501-example' }}}

---

## Size Estimation Details
- Primitives (int, float, double, etc.) are estimated based on their standard bit-packing or serialization footprint.
- Unity structs like `Vector3` and `Quaternion` are evaluated at their full uncompressed precision unless decorated with packing attributes.
- Dynamic types (strings, arrays, lists) are treated as variable, evaluating the length/header prefix as `1` byte in the static size calculation.
