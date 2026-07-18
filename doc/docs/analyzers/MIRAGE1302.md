# MIRAGE1302: Unserialized Member Warning

## The Problem
A non-public field or a property is declared inside a `[NetworkMessage]` struct or class.

Mirage's Weaver only serializes public fields. Properties (even public ones) and non-public fields (private, internal, or protected) are ignored and will not be sent over the network.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-triggering' }}}

---

## How to Resolve
Make fields public so the Weaver serializes them, or convert properties to public fields if they must be transmitted. Alternatively, ignore this warning if the member is meant to be local, or mark the field as static.

{{{ Path:'Snippets/Analyzers/Mirage1302.cs' Name:'mirage1302-resolved' }}}
