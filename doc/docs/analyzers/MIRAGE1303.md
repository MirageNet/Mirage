# MIRAGE1303: Mismatched Custom Serialization Methods

## The Problem
A custom serialization writer or reader is missing its matching counterpart, or their signatures do not match.

Mirage requires both extension methods to be defined with matching signatures. Mirage supports standard and length-based signatures:

### Standard Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader)`

### Length-based Signatures
- **Writer:** `public static void WriteMyType(this NetworkWriter writer, MyType value, int length)`
- **Reader:** `public static MyType ReadMyType(this NetworkReader reader, int length)`

If either method is missing or parameters do not align, serialization fails at compile time.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-triggering' }}}

---

## How to Resolve
Define the missing reader or writer method, ensuring that target types and signature patterns match.

{{{ Path:'Snippets/Analyzers/Mirage1303.cs' Name:'mirage1303-resolved' }}}
