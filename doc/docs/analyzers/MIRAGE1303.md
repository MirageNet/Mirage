# MIRAGE1303: Mismatched Custom Serialization Methods

## The Problem
A custom serializer signature does not match the expected pattern, or a custom reader is missing for a custom writer (or vice versa).

When writing custom serialization for a type, Mirage requires both extension methods to be defined with matching signatures. Both standard and length-based (where an integer count/length is passed as the last argument) signatures are supported:

### Standard Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader)`

### Length-based Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value, int length)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader, int length)`

If only one of the methods is defined, or if the parameter/return types do not exactly match the type or its signature pair, Mirage cannot pair them up, causing serialization to fail at compile-time.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-triggering' }}}

---

## How to Resolve

Provide a matching reader or writer method with the correct signature. Ensure that the type being read and written is exactly the same.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-resolved' }}}
