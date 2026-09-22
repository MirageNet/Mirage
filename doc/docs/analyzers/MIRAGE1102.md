# MIRAGE1102: Redundant Attribute on RPC

## The Problem
An RPC method has both a routing attribute (`[ServerRpc]` or `[ClientRpc]`) and a corresponding guard attribute (`[Server]` or `[Client]`). 

- `[Server]` is redundant on `[ServerRpc]` because the RPC body executes on the server.
- `[Client]` is redundant on `[ClientRpc]` because the RPC body executes on a receiving client, including an eligible host client.

This is an analyzer recommendation, not a combination rejected by the Weaver. Guards are injected before RPC body extraction, so the corresponding guard stays with the receiving body. It does not prevent a client from sending a ServerRpc or a server from sending a ClientRpc, and it does not select recipients.

Only these corresponding pairs are covered. An opposite-side guard, `[HasAuthority]`, `[LocalPlayer]`, or `[NetworkMethod]` can change which receiving bodies execute and must not be removed as an equivalent fix.

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-triggering' }}}

## How to Resolve
Remove the corresponding redundant guard attribute. RPC routing, target selection, and authority checks remain controlled by the RPC configuration.

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-resolved' }}}
