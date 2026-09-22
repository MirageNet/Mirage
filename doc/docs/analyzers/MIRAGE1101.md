# MIRAGE1101: Misplaced Network Attribute

## The Problem
`[SyncVar]`, `[Server]`, `[Client]`, `[ServerRpc]`, `[ClientRpc]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]` is used on a member whose declaring class does not inherit directly or indirectly from `Mirage.NetworkBehaviour`.

These attributes require a `NetworkBehaviour` instance. The Weaver reports their use in ordinary `MonoBehaviour` or plain C# classes. Nesting a helper class inside a `NetworkBehaviour` does not give the helper that inheritance.

This rule covers these specific behaviour attributes, not every Mirage attribute. For example, `[NetworkMessage]` is valid on ordinary data classes and structs. Member-target restrictions are separate: `[SyncVar]` targets fields, while the listed RPC and guard attributes target methods. Placing one directly on a property does not become valid by changing its class's base type.

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-triggering' }}}

## How to Resolve
If the class is intended to be a networked Unity component, derive it from `NetworkBehaviour` or an existing subclass. Otherwise, move the synchronized state or operation into a `NetworkBehaviour`, or remove an attribute that was added unintentionally. Changing the base type does not fix unrelated restrictions such as an invalid attribute target or a static RPC method.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-resolved' }}}
