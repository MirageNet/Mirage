# MIRAGE1102: Redundant Attribute on RPC

## When this appears

An RPC has a corresponding guard that its receiving body does not need:

| RPC | Redundant guard | Body executes on |
| --- | --- | --- |
| `[ServerRpc]` | `[Server]` | The server |
| `[ClientRpc]` | `[Client]` | A receiving client, including an eligible host client |

This is an analyzer recommendation; the Weaver accepts these combinations. Guards remain with the receiving body. They neither prevent sending from the opposite side nor select recipients.

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-triggering' }}}

## How to fix

Remove the corresponding guard. RPC configuration still controls routing, targets, and authority checks.

Only the two pairs above are covered. Opposite-side guards, `[HasAuthority]`, `[LocalPlayer]`, and `[NetworkMethod]` can change receiving execution and must not be removed as an equivalent fix.

{{{ Path:'Snippets/Analyzers/Mirage1102.cs' Name:'mirage1102-resolved' }}}
