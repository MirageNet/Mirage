# MIRAGE1403: Enabled property check on NetworkServer/Client/NetworkIdentity

## When this appears

Reading or writing Unity's inherited `enabled` property on `NetworkServer`, `NetworkClient`, or `NetworkIdentity` changes or checks component enablement, not connection or spawn state.

### Triggering example

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-triggering' }}}

## How to fix

| Check | Property |
| --- | --- |
| Server is running | `NetworkServer.Active` |
| Client is connecting or connected | `NetworkClient.Active` |
| Client is connected | `NetworkClient.IsConnected` |
| Identity has a network ID | `NetworkIdentity.IsSpawned` |

These properties are read-only. Use lifecycle and spawn/despawn APIs to change network state.

Intentional component enablement can suppress this warning. Unrelated properties named `enabled` are outside its scope.

{{{ Path:'Snippets/Analyzers/Mirage1403.cs' Name:'mirage1403-resolved' }}}
