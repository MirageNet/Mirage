# MIRAGE1206: Invalid RateLimit Attribute Settings

## When this appears

A `[ServerRpc]` has a `[RateLimit]` setting outside the allowed range.

Invalid values can stop the RPC from building, let excess calls through, or prevent the limiter from allowing calls again. Use these ranges:

| Setting | Requirement | Default |
| --- | --- | --- |
| `Interval` | Finite and greater than zero | `1f` second |
| `Refill` | Greater than zero | `50` tokens |
| `MaxTokens` | Greater than zero | `200` tokens |
| `Penalty` | Zero or greater | `1` |

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-triggering' }}}

## How to fix

Choose valid settings for the RPC's workload. `MaxTokens` controls burst capacity; replenishment is capped there, so **`MaxTokens < Refill` is valid**.

`Penalty` adds error cost when a call exceeds the limit; it is not a delay in seconds. `Penalty = 0` still rejects excess RPCs but adds no error cost.

Disconnection depends on the server's separate error-rate-limit configuration and handler.

Defaults do not guarantee protection. See [MIRAGE1207](./MIRAGE1207.md) for bucket scope, local-call behavior, and failure handling.

{{{ Path:'Snippets/Analyzers/Mirage1206.cs' Name:'mirage1206-resolved' }}}

### Scope and special cases

`Interval` must also be finite: `NaN` can let excess calls through, and positive infinity prevents refilling. This finite check is an extra analyzer rule; Weaver's checks do not reject those two values.

`[RateLimit]` configures ServerRpcs only. It has no effect on ClientRpcs or ordinary methods.

Put `[RateLimit]` on each overriding RPC. An attribute present only on the base method does not configure the override. Same-named attributes and subclasses of `RateLimitAttribute` do not replace it.
