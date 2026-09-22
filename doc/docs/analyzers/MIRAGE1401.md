# MIRAGE1401: Accessing Network State in Awake/Start

## The Problem
`Awake` and `Start` are Unity lifecycle callbacks, not Mirage spawn callbacks. This warning highlights network-dependent initialization in those methods because spawning can happen before or after `Start`. Reading `IsServer` or `IsClient` is safe when an identity exists, but a false result before spawning can cause one-time initialization to be skipped permanently. Network references can be null, and RPCs and guarded methods require their normal network preconditions.

Affected members include:
*   **Helper Properties**: `IsServer`, `IsClient`, `IsHost`, `IsLocalPlayer`, `Owner`, `HasAuthority`, `IsLocalClient`, `IsServerOnly`, `IsClientOnly`
*   **Network References**: `Server`, `Client`, `World`, `SyncVarSender`, `ServerObjectManager`, `ClientObjectManager`, `Visibility`
*   **Remote Procedure Calls**: Any method decorated with `[ServerRpc]` or `[ClientRpc]`
*   **Network Attributes**: Methods decorated with `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, or a `[NetworkMethod]` guard that requires active network state

The warning applies to direct uses of these Mirage members in `Awake` or `Start`; it is a lifecycle recommendation, not proof that every access fails. Merely obtaining `Identity` or subscribing to its lifecycle events is allowed. Calls inside event handlers are checked in their own context, not as immediate execution in the subscribing method. Deliberate state probes may be suppressed when initialization ordering is controlled.

A call must not be flagged solely for `[NetworkMethod]` when its flags include `NetworkFlags.NotActive`: that guard explicitly permits unspawned/offline execution. Other direct network-state accesses remain subject to the rule. This exception does not infer that the called method's entire body is safe, and a NotActive-only initializer should not be moved to a spawned-state callback.

Accessing `Identity.Visibility` without a custom `NetworkVisibility` component requires a server object manager and its default visibility. It can throw `InvalidOperationException` before server initialization and on client-only objects; client spawning alone does not make default visibility available.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

---

## How to Resolve
Subscribe to `Identity` lifecycle events (such as `Identity.OnStartServer`, `Identity.OnStartClient`, `Identity.OnStartLocalPlayer`, or `Identity.OnAuthorityChanged`) in `Awake` to run initialization code when the network state is ready.

Choose the event for the required state. `OnAuthorityChanged` receives a `bool`, so test it before owner-only initialization. Mirage's add-late events also invoke newly added listeners if the event has already fired. `OnStartServer` runs after the identity receives a network ID but before it is added to the world's spawned-object collection, so it is not a guarantee that every later spawn operation has completed.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}
