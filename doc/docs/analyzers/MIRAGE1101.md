# MIRAGE1101: Misplaced Network Attribute

## When this appears

`[SyncVar]`, an RPC attribute, or a network guard is used in a class that does not inherit from `NetworkBehaviour`.

These attributes use the component's network state to synchronize fields, send RPCs, or check where a method may run. An ordinary class or `MonoBehaviour` does not provide that state.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-triggering' }}}

## How to fix

For a networked Unity component, derive from `NetworkBehaviour` or an existing subclass. Otherwise, move the synchronized state or operation into a `NetworkBehaviour`, or remove an unintended attribute.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-resolved' }}}

A helper nested inside a `NetworkBehaviour` does not inherit from it. The helper itself must derive from `NetworkBehaviour`, directly or indirectly, for these attributes to work.

This rule covers `[SyncVar]`, `[ServerRpc]`, `[ClientRpc]`, `[Server]`, `[Client]`, `[HasAuthority]`, `[LocalPlayer]`, and `[NetworkMethod]`. Other attributes have different requirements: `[NetworkMessage]`, for example, supports ordinary data classes and structs.

Attribute targets still apply: `[SyncVar]` targets fields; the listed RPC and guard attributes target methods. Changing the base class does not legalize an attribute placed directly on a property or a static RPC method.
