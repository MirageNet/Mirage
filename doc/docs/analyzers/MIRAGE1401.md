# MIRAGE1401: Accessing Network State in Awake/Start

## When this appears

A network property or method is used directly in `Awake` or `Start`.

These methods can run before the object is spawned. An `IsServer` check may skip initialization, network references can still be null, and RPC calls can fail.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

## How to fix

Subscribe in `Awake` to the `Identity` event for the state you need. These events also invoke listeners added after the event has fired.

| You need | Use |
| --- | --- |
| Server state | `OnStartServer` |
| Client state | `OnStartClient` |
| Local player state | `OnStartLocalPlayer` |
| Authority | `OnAuthorityChanged`; check its Boolean argument |

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}

## Checked members

| Use | Members |
| --- | --- |
| State | `IsServer`, `IsClient`, `IsHost`, `IsLocalPlayer`, `Owner`, `HasAuthority`, `IsLocalClient`, `IsServerOnly`, `IsClientOnly` |
| References | `Server`, `Client`, `World`, `SyncVarSender`, `ServerObjectManager`, `ClientObjectManager`, `Identity.Visibility` |
| Calls | `[ServerRpc]`, `[ClientRpc]`, `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, and `[NetworkMethod]` requiring active network state |

## Notes

- Getting `Identity` and subscribing to events are allowed. This warning does not treat event-handler code as part of the `Awake` or `Start` body.
- A `[NetworkMethod]` call allowing `NetworkFlags.NotActive` is exempt for that guard. Other direct network-state uses still count. Keep NotActive-only initialization outside spawned-state callbacks.
- Suppress the warning for deliberate state checks when you control initialization order.
- `OnStartServer` runs before the object is added to `World`.
- Without a custom `NetworkVisibility` component, `Identity.Visibility` needs an initialized server object manager. Spawning on a client alone is not enough.
