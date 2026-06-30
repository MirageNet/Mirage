# MIRAGE1401: Accessing Network State in Awake/Start

## The Problem

Reading, writing, or invoking any of the following network properties, fields, or methods inside Unity's lifecycle methods `Awake` or `Start`:

*   **Helper Properties**: `IsServer`, `IsClient`, `IsHost`, `IsLocalPlayer`, `IsOwner`, `HasAuthority`
*   **Network References**: `Server`, `Client`, `World`, `SyncVarSender`, `ServerObjectManager`, `ClientObjectManager`, `Visibility`
*   **Remote Procedure Calls**: Any method decorated with `[ServerRpc]` or `[ClientRpc]`
*   **Network Attributes**: Methods decorated with `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]`

Unity's `Awake` and `Start` methods are called during GameObject initialization before Mirage's network identity is spawned or initialized. At this point, properties and fields representing the network state or references are not set (they are null or default). Accessing them, invoking RPC methods, or calling attribute-guarded methods inside `Awake` or `Start` leads to incorrect behavior, `NullReferenceException` at runtime, default values, or race conditions.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-triggering' }}}

---

## How to Resolve

Subscribe to lifecycle events on `Identity` (such as `Identity.OnStartServer`, `Identity.OnStartClient`, `Identity.OnStartLocalPlayer`, or `Identity.OnAuthorityChanged`) during `Awake()` to execute your network initialization code when the network state is fully ready.

{{{ Path:'Snippets/Analyzers/Mirage1401.cs' Name:'mirage1401-resolved' }}}

