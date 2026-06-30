# MIRAGE1305: Missing NetworkMessage Attribute

## The Problem
A class or struct is used as a network message (passed as the type argument to one of the message-related API methods listed below), but it is missing the `[NetworkMessage]` attribute.

Mirage's post-compilation Weaver generates serialization code and registers a unique message type ID (a 16-bit stable hash of the full type name) for every type marked `[NetworkMessage]`. When a type is used in a message-related call **without** that attribute, the Weaver may not produce the required serialization helpers, which results in runtime errors such as failing to serialize, failing to unpack, or "Unexpected message ID" warnings.

### Affected Methods

The following generic methods all require their type argument to carry `[NetworkMessage]`:

| Method | Declared On |
|---|---|
| `Send<T>()` | `INetworkPlayer`, `NetworkPlayer`, `NetworkClient` |
| `RegisterHandler<T>()` | `IMessageReceiver`, `MessageHandler` |
| `UnregisterHandler<T>()` | `IMessageReceiver`, `MessageHandler` |
| `SendToAll<T>()` | `NetworkServer` |
| `SendToMany<T>()` | `NetworkServer` |
| `Pack<T>()` | `MessagePacker` |
| `Unpack<T>()` | `MessagePacker` |
| `GetId<T>()` | `MessagePacker` |

### Same-Assembly vs Cross-Assembly Behavior

**Same assembly:** The Weaver scans every invocation of the methods above and can generate reader/writer code on the fly for the type argument even if `[NetworkMessage]` is absent. The type will still work at runtime, but the warning is raised because the intent is ambiguous and the behavior is fragile.

**Cross-assembly (most critical case):** When a message type is defined in one assembly (e.g., a shared library or a separate asmdef) and *used* in another, the Weaver can only generate its serialization helpers in the **defining assembly**. That only happens when the type is tagged `[NetworkMessage]` in its own assembly. Without it, no reader/writer is generated in the defining assembly, so any assembly that references it will fail at runtime with a missing serializer error.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

---

## How to Resolve

Decorate the target message class or struct with the `[NetworkMessage]` attribute. This instructs Mirage's Weaver to generate serialization code and register the type ID in the assembly where the type is defined, making it safe to use across assembly boundaries.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}
