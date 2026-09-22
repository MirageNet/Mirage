# MIRAGE1101: Misplaced Network Attribute

## When this appears

`[SyncVar]`, `[Server]`, `[Client]`, `[ServerRpc]`, `[ClientRpc]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]` appears on a member whose declaring class does not inherit from `Mirage.NetworkBehaviour`, directly or indirectly.

These attributes require a `NetworkBehaviour` instance. A helper nested inside a `NetworkBehaviour` does not inherit from it. Other Mirage attributes have different requirements: `[NetworkMessage]`, for example, supports ordinary data classes and structs.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-triggering' }}}

## How to fix

For a networked Unity component, derive from `NetworkBehaviour` or an existing subclass. Otherwise, move the synchronized state or operation into a `NetworkBehaviour`, or remove an unintended attribute.

Attribute targets still apply: `[SyncVar]` targets fields; the listed RPC and guard attributes target methods. Changing the base class does not legalize an attribute placed directly on a property or a static RPC method.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-resolved' }}}
