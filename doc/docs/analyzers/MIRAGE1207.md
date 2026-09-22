# MIRAGE1207: Missing RateLimit on ServerRpc

## When this appears

A `[ServerRpc]` lacks a recognized Mirage `[RateLimit]` on its declaration. This advisory recommends a request budget; the Weaver does not require one.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-triggering' }}}

## How to fix

Add `[RateLimit]` with an appropriate budget; see [MIRAGE1206](./MIRAGE1206.md) for settings, penalties, and attribute recognition. If independent server-side throttling supplies the intended protection, use normal diagnostic suppression; the analyzer does not inspect arbitrary limiter code.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-resolved' }}}

### Budget and scope

Buckets are per player, concrete runtime `NetworkBehaviour` type, and RPC index. Instances of that type share each RPC's budget; subclasses with inherited RPCs have separate buckets. This is not a global or persistent account limit.

Buckets start with `MaxTokens`. Every checked attempt consumes a token, including rejections, allowing debt. Time-based refill is capped at capacity.

The example starts with ten tokens and refills five per 0.2 seconds: a refill budget of 25 per second, not a fixed-window quota.

### Calls and failures

- **Remote client:** excess calls throw `Mirage.RemoteCalls.ReturnRpcException` before sending, even for `void` RPCs. Pace calls or handle the exception.
- **Server:** independently checks its separate bucket after authority validation, before argument deserialization and execution. Excess `void` calls are dropped; excess result RPCs receive a failure reply that fails their task.
- **Local:** host-local calls and server-local calls with `allowServerToCall` bypass the limiter. Enforce gameplay cooldowns in server logic.

Keep authorization, input validation, payload-size limits, and asynchronous concurrency limits. Incoming packets still incur receipt and validation costs.
