# MIRAGE1305: Missing NetworkMessage Attribute

## When this appears

A concrete message class or struct in your project is missing `[NetworkMessage]` when used with one of the message APIs listed below.

Without the attribute, a message used through a generic wrapper or reflection may have no serializers at runtime. The attribute tells Weaver to prepare its serializers when building the message's assembly.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

## How to fix

Add `[NetworkMessage]` to the message class or struct. The type and all fields you send must still be serializable. Types used only as fields or RPC arguments do not need the attribute.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}

## Message APIs

| Method | Declared on |
|---|---|
| `Send<T>()` | `IMessageSender` (inherited by `INetworkPlayer`), `NetworkPlayer`, `NetworkClient` |
| `RegisterHandler<T>()` | `IMessageReceiver`, `MessageHandler`, `MessageReceiverExtensions` |
| `UnregisterHandler<T>()` | `IMessageReceiver`, `MessageHandler` |
| `SendToAll<T>()`, `SendToMany<T>()` | `NetworkServer` |
| `Pack<T>()`, `Unpack<T>()`, `GetId<T>()` | `MessagePacker` |

Extension calls to these APIs count; unrelated methods with the same names do not. `GetId<T>()` and `UnregisterHandler<T>()` are also checked, although they do not need serializers themselves.

## Discovery details

`[NetworkMessage]` also registers the type for diagnostic messages and checks for message ID collisions. It does not install a receiving handler: use `RegisterHandler<T>()` separately. `GetId<T>()` computes an ID from the type name without checking the attribute.

Weaver can generate serializers for concrete types used in direct generic calls to the APIs above, except `MessageReceiverExtensions.RegisterHandler`. It generates them in the calling assembly, including for accessible types from other assemblies.

Custom or manually initialized serializers can also support messages without the attribute. Weaver skips open generic arguments; arbitrary generic wrappers and reflection do not cause it to generate serializers for each concrete type.

You cannot add the attribute to framework types or types from assemblies you cannot edit, and Weaver rejects it on open generic definitions. Use a concrete wrapper in your project, or ensure serializers are available before suppressing this rule.
