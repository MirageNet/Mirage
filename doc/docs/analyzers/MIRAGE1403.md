# MIRAGE1403: Enabled property check on NetworkServer/Client/NetworkIdentity

## When this appears

Code reads or writes `.enabled` on a `NetworkServer`, `NetworkClient`, or `NetworkIdentity`.

`enabled` controls the Unity component, not the connection or spawn state. An enabled component can still have no running server, connected client, or spawned identity.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

## How to fix

| Check | Property |
| --- | --- |
| Server is running | `NetworkServer.Active` |
| Client is connecting or connected | `NetworkClient.Active` |
| Client is connected | `NetworkClient.IsConnected` |
| Identity has a network ID | `NetworkIdentity.IsSpawned` |

These properties are read-only. Use server/client start and stop methods, or object-manager spawn/despawn methods, to change network state.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}

If you mean to check or change the Unity component's enabled state, you can suppress this warning. It does not apply to unrelated properties named `enabled`.
