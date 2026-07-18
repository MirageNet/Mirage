# MIRAGE1305: Missing NetworkMessage Attribute

## The Problem
A class or struct is used as a network message in one of the API methods below, but lacks the `[NetworkMessage]` attribute.

Mirage's Weaver generates serialization code and registers message IDs for types marked with `[NetworkMessage]`. Using a type without this attribute can cause runtime errors, including serialization failures, unpacking issues, or "Unexpected message ID" warnings.

### Affected Methods
The following generic methods require their type argument to have `[NetworkMessage]`:

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
* **Same assembly:** The Weaver can generate readers and writers on the fly during compilation even if `[NetworkMessage]` is missing. The code will work, but raises a warning to prevent fragile or ambiguous behavior.
* **Cross-assembly:** When a message is defined in one assembly and used in another, the Weaver must generate the serialization code in the defining assembly. This requires `[NetworkMessage]` on the definition. Without it, referencing assemblies will fail at runtime with missing serializer errors.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-triggering' }}}

---

## How to Resolve
Add the `[NetworkMessage]` attribute to the message class or struct. This ensures the Weaver generates the required serialization code in the defining assembly.

{{{ Path:'Snippets/Analyzers/Mirage1305.cs' Name:'mirage1305-resolved' }}}
