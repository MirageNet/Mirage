# MIRAGE1102: Redundant Server/Client Attribute on RPC

## The Problem
An RPC method is decorated with both a routing attribute (`[ServerRpc]` or `[ClientRpc]`) and its corresponding active guard attribute (`[Server]` or `[Client]`).

- Declaring `[Server]` on a method marked with `[ServerRpc]` is redundant because a ServerRpc can only execute on the server.
- Declaring `[Client]` on a method marked with `[ClientRpc]` is redundant because a ClientRpc can only execute on clients.

Adding both attributes triggers unnecessary active guard injection during weaving, increases code clutter, and can cause confusion about the method's lifecycle.

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-triggering' }}}

## How to Resolve
Remove the redundant active guard attribute (`[Server]` or `[Client]`) from the RPC method.

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-resolved' }}}
