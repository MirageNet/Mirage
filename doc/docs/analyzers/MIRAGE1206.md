# MIRAGE1206: Invalid RateLimit Attribute Settings

## When this appears

A recognized Mirage `[RateLimit]` on a `[ServerRpc]` violates these requirements:

| Setting | Requirement | Default |
| --- | --- | --- |
| `Interval` | Finite and greater than zero | `1f` second |
| `Refill` | Greater than zero | `50` tokens |
| `MaxTokens` | Greater than zero | `200` tokens |
| `Penalty` | Zero or greater | `1` |

Finite intervals are an additional analyzer requirement beyond the Weaver's comparison checks: `NaN` and either infinity are invalid. NaN can stop rejection; positive infinity prevents replenishment.

`[RateLimit]` only configures ServerRpcs, not ClientRpcs or ordinary methods. Apply Mirage's exact attribute directly to each overriding RPC; same-named attributes, derived substitutes, and base-only attributes do not configure it.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-triggering' }}}

## How to fix

Choose valid settings for the RPC's workload. `MaxTokens` controls burst capacity; replenishment is capped there, so **`MaxTokens < Refill` is valid**.

`Penalty` adds error cost on an excess call, not a delay in seconds. `Penalty = 0` keeps RPC throttling enabled and adds no error cost.

Disconnection depends on the server's separate error-rate-limit configuration and handler.

Defaults do not guarantee protection. See [MIRAGE1207](./MIRAGE1207.md) for bucket scope, local-call behavior, and failure handling.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-resolved' }}}
