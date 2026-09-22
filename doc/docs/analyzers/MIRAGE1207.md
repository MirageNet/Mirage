# MIRAGE1207: Missing RateLimit on ServerRpc

## The Problem
A `[ServerRpc]` method lacks a recognized Mirage `[RateLimit]` on its declaration. This is an advisory analyzer policy: the Weaver permits ServerRpcs without a rate limit.

An authorized client can still send too many otherwise valid requests. A suitable rate limit reduces how often a remotely received RPC body runs, but does not replace sender authorization, payload-size limits, input validation, or limits on concurrent asynchronous work. A same-named custom attribute or a RateLimit on a base declaration does not configure an overriding RPC.

---

## Example of Triggering Code
{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-triggering' }}}

---

## How to Resolve
Add `[RateLimit]` with a budget appropriate to this RPC. Validate its settings with [MIRAGE1206](./MIRAGE1206.md). If independent server-side throttling already provides the intended protection, review it and suppress this advisory using normal diagnostic configuration; the analyzer does not inspect arbitrary limiter code.

{{{ Path:'Snippets/Analyzers/Mirage1207.cs' Name:'mirage1207-resolved' }}}

### What the limit covers

Each player has a bucket for the concrete runtime `NetworkBehaviour` type and RPC index. Calls to the same RPC on multiple instances of that component type share the bucket; different players and different concrete types have separate buckets. Inherited RPCs on different concrete subclasses do not share a single base-method budget. This is not a global or persistent account-level limit.

The bucket starts with `MaxTokens`. Each checked attempt consumes one token, including rejected attempts, which can leave the bucket in debt. Replenishment is time-based and capped at capacity. The example allows an initial burst of ten calls and refills five tokens per 0.2 seconds (a refill budget of 25 per second); it is not a strict fixed-window quota.

The normal remote client checks its own bucket and throws `Mirage.RemoteCalls.ReturnRpcException` before sending when it exceeds the limit, including for `void` RPCs. Handle this possibility or pace calls appropriately. The server checks its separate bucket after authority validation and before deserializing RPC arguments and invoking the body, so bypassing a modified client's local check does not bypass the server check. Excess void calls are dropped; an excess return RPC receives a failure reply that fails its task.

Host-local invocation and server-local invocation enabled by `allowServerToCall` execute the body directly and bypass this limiter. The attribute therefore does not enforce a gameplay cooldown consistently across local and remote execution. Enforce such game rules in server logic as well.

`Penalty` adds error cost when a checked call exceeds its budget. Whether that eventually disconnects a player depends on the server's separate error-rate-limit configuration and handler. Rate limiting also does not eliminate the cost of receiving and validating incoming packets.
