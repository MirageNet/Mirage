# MIRAGE1101: Misplaced Network Attribute

## The Problem
A Mirage network attribute (such as `[SyncVar]`, `[Server]`, `[Client]`, `[ServerRpc]`, `[ClientRpc]`, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]`) is used in a class that does not inherit from `NetworkBehaviour`.

These attributes require the context of a `NetworkBehaviour` instance. Using them in standard `MonoBehaviour` or plain C# classes causes compile or Weaver errors.

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-triggering' }}}

## How to Resolve
Change the class to inherit from `NetworkBehaviour` instead of `MonoBehaviour` or other base classes.

{{{ Path:'Snippets/Analyzers/Mirage1101.cs' Name:'mirage1101-resolved' }}}
