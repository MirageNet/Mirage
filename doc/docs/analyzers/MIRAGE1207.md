# MIRAGE1207: Missing RateLimit on ServerRpc

## When this appears

A `[ServerRpc]` method is missing `[RateLimit]`.

Without a rate limit, clients can send too many requests, causing server lag, memory pressure, or disconnects.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-triggering' }}}

## How to fix

Add `[RateLimit]` and choose a burst size and refill rate that fit the work done by the RPC. See [MIRAGE1206](./MIRAGE1206.md) for valid settings, penalties, and where to apply the attribute.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-resolved' }}}

### How calls are counted

Each player has a separate token bucket for each concrete runtime `NetworkBehaviour` type and RPC index. Instances of the same type share that RPC's limit; subclasses with inherited RPCs have separate buckets.

This is not a global or persistent account limit.

Each bucket starts with `MaxTokens`. Every checked attempt uses a token, even if rejected, so repeated rejected calls can take the count below zero. Tokens refill over time, up to the capacity.

The example starts with ten tokens and refills five per 0.2 seconds: a refill rate of 25 tokens per second. It does not enforce a fixed window of five calls.

### Calls and failures

- **Remote client:** excess calls throw `Mirage.RemoteCalls.ReturnRpcException` before sending, even for `void` RPCs. Pace calls or handle the exception.
- **Server:** checks its own bucket after checking authority, before reading the RPC arguments or running the method. Excess `void` calls are dropped; result RPCs get a failure reply that fails the caller's task.
- **Local:** host-local calls and server-local calls with `allowServerToCall` bypass the limiter. Enforce gameplay cooldowns in server logic.

Keep checking permissions and inputs in server code, and limit payload sizes and concurrent asynchronous work. Packets still take work to receive and validate.

Weaver allows ServerRpcs without this attribute. If separate server logic already limits these requests, use normal diagnostic suppression after checking that protection. The analyzer does not inspect custom limiter code.
