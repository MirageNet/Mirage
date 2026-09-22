# MIRAGE1305: Missing NetworkMessage Attribute

## The Problem
A project-owned concrete class or struct is used as a message without an explicit `[NetworkMessage]` declaration. Requiring the marker is an **analyzer convention** that makes message discovery explicit; it is not a universal runtime requirement.

The marker tells the Weaver to obtain readers and writers and register the message type when weaving its defining assembly. The type and all transmitted values must still be serializable.

### Affected Methods
This convention covers the following Mirage message APIs. Match their actual method symbols, including reduced extension methods, rather than unrelated methods with the same names.

| Method | Declared On |
|---|---|
| `Send<T>()` | `IMessageSender` (inherited by `INetworkPlayer`), `NetworkPlayer`, `NetworkClient` |
| `RegisterHandler<T>()` | `IMessageReceiver`, `MessageHandler`, `MessageReceiverExtensions` |
| `UnregisterHandler<T>()` | `IMessageReceiver`, `MessageHandler` |
| `SendToAll<T>()` | `NetworkServer` |
| `SendToMany<T>()` | `NetworkServer` |
| `Pack<T>()` | `MessagePacker` |
| `Unpack<T>()` | `MessagePacker` |
| `GetId<T>()` | `MessagePacker` |

### What the Weaver Discovers

Recognized direct generic calls with concrete type arguments can trigger serialization generation in the calling assembly, including for accessible types from another assembly. Therefore, an unmarked message can work in either case. Custom or manually initialized serializers can also supply the required serialization.

Discovery is limited: open generic arguments are skipped, and arbitrary generic wrappers or reflection do not cause whole-program specialization. The convenience `MessageReceiverExtensions.RegisterHandler` overloads are not direct discovery triggers in the current Weaver. Explicit marking avoids relying on those call paths to discover a project-owned message.

### IDs and Handlers

`MessagePacker.GetId<T>()` computes an ID from the type name without checking this attribute. Weaver message registration records the type for diagnostics and collision detection. `RegisterHandler<T>()` separately installs a receiving handler. Adding the attribute does not install a handler or fix an unexpected-message warning caused by a missing handler. `GetId<T>()` and `UnregisterHandler<T>()` are included here as a convention, even though their runtime operations do not require serialization.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

---

## How to Resolve
Add `[NetworkMessage]` to the editable concrete message declaration. Nested payload types used only as fields or RPC arguments do not need the marker.

Do not offer an attribute code fix for framework types, declarations supplied only by referenced metadata, or an open generic definition: the Weaver rejects `[NetworkMessage]` on open generic types. Where the declaration cannot be marked, use a project-owned concrete message wrapper or an explicitly established serializer/discovery path and suppress this convention as appropriate.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}
