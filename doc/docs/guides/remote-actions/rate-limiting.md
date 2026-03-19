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
```csharp
public class PlayerSocial : NetworkBehaviour
{
    // Allows 1 emote every 2 seconds, but the player can burst up to 3 emotes consecutively.
    // Spamming emotes too fast will drop the call and apply a penalty to their error limit.
    // Penalty set to 0 to not penalize the player for sending too many emotes, we can just ignore them.
    [ServerRpc]
    [RateLimit(Interval = 2f, Refill = 1, MaxTokens = 3, Penalty = 0)]
    public void CmdSendEmote(int emoteId)
    {
        // ... process and broadcast emote to other players ...
    }
}
```

#### Malicious Spam (High Penalty)
Hackers or scripts sometimes flood servers with positional updates or action requests. Using a `Penalty` parameter, we can rapidly trigger a global server disconnect.

```csharp
public class PlayerRTSController : NetworkBehaviour
{
    // Players should realistically only be issuing a few move commands a second.
    // We allow a healthy burst of 10 commands if they are furiously clicking across the map.
    // However, if a cheat sends 100 move instructions instantly, the large Penalty (10) 
    // will quickly exceed the global error threshold and kick them.
    [ServerRpc]
    [RateLimit(Interval = 1f, Refill = 5, MaxTokens = 10, Penalty = 10)]
    public void CmdMoveUnit(NetworkIdentity unit, Vector3 targetPosition)
    {
        // ... process movement command ...
    }
}
```

## Configuration

`[RateLimit]` attribute has the following parameters:

| Parameter   | Default | Description |
|-------------|---------|-------------|
| `Interval`  | 1f      | Time (in seconds) between token refills. |
| `Refill`    | 50      | Tokens added to the bucket every `Interval`. |
| `MaxTokens` | 200     | Maximum capacity of the bucket. Setting this higher than `Refill` allows a client to save tokens for rapid bursts. |
| `Penalty`   | 1       | Error cost applied to the player's global `ErrorRateLimit` when they exceed this RPC's limit. |

Values are best set on a case-by-case basis depending on the RPC. Some RPCs, like movement instructions, might be expected to be called every frame, whereas others like sending chat messages or changing weapons should only happen occasionally.
