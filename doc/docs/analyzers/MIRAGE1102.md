# MIRAGE1102: Redundant Attribute on RPC

## When this appears

A `[ServerRpc]` method also has `[Server]`, or a `[ClientRpc]` method also has `[Client]`.

The RPC already ensures its body runs on the server or a receiving client. The extra guard adds a duplicate check and can be mistaken for a restriction on who may send the RPC.

| RPC | Redundant guard | Body executes on |
| --- | --- | --- |
| `[ServerRpc]` | `[Server]` | The server |
| `[ClientRpc]` | `[Client]` | A receiving client, including an eligible host client |

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-triggering' }}}

## How to fix

Remove the corresponding guard. RPC configuration still controls routing, targets, and authority checks.

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-resolved' }}}

This rule covers only the two pairs above. Opposite-side guards, `[HasAuthority]`, `[LocalPlayer]`, and `[NetworkMethod]` can change whether the receiving body runs; keep them unless you intend that change.

Guards stay with the receiving body. They neither prevent sending from the opposite side nor select recipients. Weaver accepts these combinations; the analyzer recommends removing the redundant guard.
