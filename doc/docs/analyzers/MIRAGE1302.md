# MIRAGE1302: Unserialized Member Warning

## The Problem
A non-public field or property is declared inside a `[NetworkMessage]` struct or class. 

In Mirage, all properties (including public ones) and internal/protected fields are ignored by the Weaver during automatic serialization. Only public fields are serialized and sent over the network. If a developer declares properties or non-public fields expecting them to be networked, it can lead to confusion and logic bugs because they will remain uninitialized or hold default values on the receiving end.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

---

## How to Resolve

Make the field public so it is automatically picked up by the Weaver for serialization. Properties should be converted to public fields if they need to be transmitted. If the member is intended to be purely local and not serialized, you can ignore this warning, or mark the field as static if applicable.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}
