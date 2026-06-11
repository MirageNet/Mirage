---
sidebar_position: 4
---
# Rate Limiting

Rate Limiting protects your server from being flooded by excessive `[ServerRpc]` calls from clients. By default, there is no rate limiting, so you must explicitly opt-in per RPC using the `[RateLimit]` attribute.

## How It Works

The rate limiter uses a Token Bucket algorithm that tracks each client's calls to specific RPCs. 
Calling the RPC consumes a token. The server then automatically refills your bucket of tokens based on the configured interval. 

If a client calls an RPC when their bucket is empty (sending calls too fast), the RPC is silently dropped. An error penalty is also applied to that player's error limit, which can lead to a disconnect if the player accumulates too many errors. For more details on how these penalties work, see the [Error Handling](../error-handling) guide.

**Per-RPC Limits (Not Per-Object)**
Rate limits track each RPC method globally for that specific player, across all instances of that class. For example, maxing out the limit on `CmdSwitchWeapon` will not stop a player from calling `CmdJump`. However, if the player tries to invoke `CmdInteract` on 5 different doors in the scene, all 5 interactions will count towards the same `CmdInteract` limit.

:::note
`[RateLimit]` only works on `[ServerRpc]` methods.
:::

:::note
In Host mode, rate limits are ignored for the local player, since they cannot flood their own server.
:::

### Examples

#### Harmless Action (No Penalty)
{{{ Path:'Snippets/RemoteActions/RateLimitingExamples.cs' Name:'rate-limiting-emote' }}}

#### Malicious Spam (High Penalty)
Hackers or scripts sometimes flood servers with positional updates or action requests. Using a `Penalty` parameter, we can rapidly trigger a global server disconnect.

{{{ Path:'Snippets/RemoteActions/RateLimitingExamples.cs' Name:'rate-limiting-move' }}}

## Configuration

`[RateLimit]` attribute has the following parameters:

| Parameter   | Default | Description |
|-------------|---------|-------------|
| `Interval`  | 1f      | Time (in seconds) between token refills. |
| `Refill`    | 50      | Tokens added to the bucket every `Interval`. |
| `MaxTokens` | 200     | Maximum capacity of the bucket. Setting this higher than `Refill` allows a client to save tokens for rapid bursts. |
| `Penalty`   | 1       | Error cost applied to the player's global `ErrorRateLimit` when they exceed this RPC's limit. |

Values are best set on a case-by-case basis depending on the RPC. Some RPCs, like movement instructions, might be expected to be called every frame, whereas others like sending chat messages or changing weapons should only happen occasionally.
