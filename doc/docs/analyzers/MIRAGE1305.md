# MIRAGE1305: Missing NetworkMessage Attribute

## When this appears

A project-owned concrete message class or struct lacks an explicit `[NetworkMessage]` declaration when using these Mirage APIs. This marker requirement is an **analyzer convention**.

| Method | Declared on |
|---|---|
| `Send<T>()` | `IMessageSender` (inherited by `INetworkPlayer`), `NetworkPlayer`, `NetworkClient` |
| `RegisterHandler<T>()` | `IMessageReceiver`, `MessageHandler`, `MessageReceiverExtensions` |
| `UnregisterHandler<T>()` | `IMessageReceiver`, `MessageHandler` |
| `SendToAll<T>()`, `SendToMany<T>()` | `NetworkServer` |
| `Pack<T>()`, `Unpack<T>()`, `GetId<T>()` | `MessagePacker` |

Extension calls count; unrelated namesakes do not. `GetId<T>()`/`UnregisterHandler<T>()` are included by convention despite requiring no serialization.

`[NetworkMessage]` obtains serializers and registers the type for diagnostics and collision detection when weaving its defining assembly.

Recognized direct generic calls with concrete arguments can generate serializers in the calling assembly, including for accessible external types. Custom or manually initialized serializers can also support unmarked messages.

Open generic arguments are skipped. Arbitrary generic wrappers/reflection do not cause specialization; convenience `MessageReceiverExtensions.RegisterHandler` overloads are not direct discovery triggers.

- `GetId<T>()` computes a type-name ID without checking the marker.
- `RegisterHandler<T>()` separately installs the receiving handler. The marker cannot fix a missing-handler warning.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

## How to fix

Mark the editable concrete message declaration. The type and all transmitted values must still be serializable. Payloads used only as fields or RPC arguments do not need the marker.

Framework and metadata-only types cannot receive this attribute fix; the Weaver rejects marked open generic definitions. Use a project-owned concrete wrapper, or establish a serializer/discovery path and suppress the convention where appropriate.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}
