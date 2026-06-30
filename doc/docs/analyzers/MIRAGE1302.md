# MIRAGE1302: Unserialized Private Field Warning

## The Problem
A private field or property is declared inside a `[NetworkMessage]` struct or class. 

In Mirage, private fields and properties are ignored by the Weaver during automatic serialization. Only public fields are serialized and sent over the network. If a developer declares private fields expecting them to be networked, it can lead to confusion and logic bugs because they will remain uninitialized or hold default values on the receiving end.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

---

## How to Resolve

Make the field public so it is automatically picked up by the Weaver for serialization. If the field is intended to be purely local and not serialized, you can ignore this warning, or mark it as static if applicable.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}
