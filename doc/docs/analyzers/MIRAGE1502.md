# MIRAGE1502: Unbounded String or Collection

## The Problem
A network message field or RPC parameter contains a `string`, `List<T>`, `T[]` array, or other collection without specifying a maximum size/length limit.

Allowing unbounded strings or collections in network messages introduces security risks (such as memory exhaustion attacks, out-of-memory crashes, or denial of service) if a client sends an extremely large payload.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1502.cs' Name:'mirage1502-triggering' }}}

---

## How to Resolve

Use size-limiting attributes (such as `[BitCount]` or other string/collection size limiters) to restrict the collection size at serialization time, or enforce maximum limits during deserialization (such as setting `MaxDeltaCount` or `MaxElements` on SyncObjects).

{{{ Path:'Snippets/Analyzers/Mirage1502.cs' Name:'mirage1502-resolved' }}}
