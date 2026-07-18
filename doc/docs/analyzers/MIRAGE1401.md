# MIRAGE1401: Accessing Network State in Awake/Start

## The Problem
Do not access network properties, references, or methods inside Unity's `Awake` or `Start`. At this stage, Mirage's network identity is not yet spawned or initialized, so these values are null or default. Calling RPCs or accessing these fields will cause `NullReferenceException`, default values, or race conditions.

Affected members include:
*   **Helper Properties**: `IsServer`, `IsClient`, `IsHost`, `IsLocalPlayer`, `Owner`, `HasAuthority`, `IsLocalClient`, `IsServerOnly`, `IsClientOnly`
*   **Network References**: `Server`, `Client`, `World`, `SyncVarSender`, `ServerObjectManager`, `ClientObjectManager`, `Visibility`
*   **Remote Procedure Calls**: Any method decorated with `[ServerRpc]` or `[ClientRpc]`
*   **Network Attributes**: Methods decorated with `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]`

Additionally, accessing `Visibility` without a custom `NetworkVisibility` component requires `ServerObjectManager`, which is null before spawning and throws `InvalidOperationException`.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

---

## How to Resolve
Subscribe to `Identity` lifecycle events (such as `Identity.OnStartServer`, `Identity.OnStartClient`, `Identity.OnStartLocalPlayer`, or `Identity.OnAuthorityChanged`) in `Awake` to run initialization code when the network state is ready.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}

