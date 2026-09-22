# MIRAGE1401: Accessing Network State in Awake/Start

## When this appears

Network-dependent initialization directly in `Awake` or `Start` can run before spawning. State checks may skip initialization, references may be null, and RPCs or guarded methods may fail. Spawning can also happen before `Start`; this warning identifies an ordering risk.

| Use | Affected members |
| --- | --- |
| State | `IsServer`, `IsClient`, `IsHost`, `IsLocalPlayer`, `Owner`, `HasAuthority`, `IsLocalClient`, `IsServerOnly`, `IsClientOnly` |
| References | `Server`, `Client`, `World`, `SyncVarSender`, `ServerObjectManager`, `ClientObjectManager`, `Identity.Visibility` |
| Calls | `[ServerRpc]`, `[ClientRpc]`, `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, and `[NetworkMethod]` requiring active network state |

Exceptions:

- Getting `Identity` and subscribing to its events are allowed. Callback bodies are not immediate initialization code.
- A `[NetworkMethod]` call permitting `NetworkFlags.NotActive` is exempt for that guard; other direct network-state accesses still count. Keep NotActive-only initialization outside spawned-state callbacks.
- Suppress this recommendation for deliberate state probes when you control initialization order.

### Triggering example

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

## How to fix

Subscribe in `Awake` to the appropriate `Identity` event. Add-late events also invoke listeners registered after the event fired.

| Required state | Event |
| --- | --- |
| Server | `OnStartServer` |
| Client | `OnStartClient` |
| Local player | `OnStartLocalPlayer` |
| Authority | `OnAuthorityChanged`; check its Boolean argument |

`OnStartServer` runs before world registration. Default `Identity.Visibility` requires an initialized server object manager; client spawning alone is insufficient without a custom visibility component.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}
