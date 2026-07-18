# MIRAGE1102: Redundant Attribute on RPC

## The Problem
An RPC method has both a routing attribute (`[ServerRpc]` or `[ClientRpc]`) and a corresponding guard attribute (`[Server]` or `[Client]`). 

- `[Server]` is redundant on `[ServerRpc]` since ServerRpcs only run on the server.
- `[Client]` is redundant on `[ClientRpc]` since ClientRpcs only run on clients.

Combining them causes redundant guard code generation during weaving and clutters the codebase.

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-triggering' }}}

## How to Resolve
Remove the redundant guard attribute (`[Server]` or `[Client]`).

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-resolved' }}}
