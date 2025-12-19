---
sidebar_position: 15
---
# Error Handling

Mirage includes a robust error handling system to manage issues caused by players, such as invalid messages or exploits. This system helps protect your server from crashes and allows for custom logic to handle problematic players.

## Overview

The server tracks errors caused by player actions, like malformed RPCs. If a player accumulates too many errors within a certain timeframe,  they can be automatically disconnected or handled by a custom callback.

:::note
This rate-limiting system is server-side only and replaces the old `DisconnectOnException` functionality on the server. The client still has a simple `DisconnectOnException` toggle for its own error handling.
:::

## Player Error Flags

The `PlayerErrorFlags` enum helps categorize the types of errors a player can cause, allowing for more granular tracking and response.

```csharp
// see PlayerErrorFlags in the source code for most up-to-date values
[Flags]
public enum PlayerErrorFlags
{
    None = 0,

    // Likely developer bugs
    RpcNullException = 1 << 0,
    RpcException = 1 << 1,

    // Connection/versioning issues
    DeserializationException = 1 << 2,
    RpcSync = 1 << 3,
    RateLimit = 1 << 4,

    // Security/Malicious Intent
    Unauthorized = 1 << 5,
    Critical = 1 << 6,
    LikelyCheater = 1 << 7,

    // Custom developer defined errors
    CustomError = 1 << 16
}
```

You can use these flags to identify an error's cause when implementing custom logic. You can also define your own flags using the `CustomError` bit as a starting point:

```csharp
public static class MyErrorFlags 
{
    public const PlayerErrorFlags InvalidTrade = PlayerErrorFlags.CustomError << 0;
    public const PlayerErrorFlags AnotherCustom = PlayerErrorFlags.CustomError << 1;
}
```

## Server Configuration

### How Error Rate Limiting Works

Mirage uses a token bucket algorithm to manage player errors. Each player has a "bucket" of tokens that represents their error budget.

*   **Tokens**: When a player causes an error, a `cost` is deducted from their token bucket.
*   **Max Tokens**: The capacity of the bucket. A player starts with this many tokens and cannot exceed this limit. A higher limit allows for a burst of errors, for example if something goes wrong in the game for a short amount of time.

*   **Refill & Interval**: Tokens are replenished over time. `Refill` specifies how many tokens are restored every `Interval` (in seconds).
*   **Cost**: Represents the severity of an error. When an error occurs, this amount is subtracted from the player's tokens.
*   **Reaching the Limit**: If a player's token count drops below zero, they have exhausted their budget. This triggers the error handling logic, which is a disconnect by default.

This system tolerates occasional minor errors while penalizing frequent or severe infractions.

On the `NetworkServer` component, you can configure this behavior:

-   **Error Rate Limit Enabled**: Toggles the rate-limiting feature. Enabled by default.
-   **Error Rate Limit Config**: Configures the token bucket (`MaxTokens`, `Refill`, `Interval`).
-   **Rethrow Exception**: If enabled, exceptions are re-thrown after being logged. This is for debugging and can interrupt server operations.

## Manual Error Reporting

You can manually trigger an error for a player from server-side code using `INetworkPlayer.SetError`.

The `cost` parameter specifies how many tokens to subtract from the player's error bucket. A higher cost leads to faster rate-limiting. Setting a cost higher than the player's current tokens (or even `MaxTokens`) will trigger the error limit immediately.

### Custom Error Example

```csharp
public static class MyErrorFlags 
{
    public const PlayerErrorFlags InvalidAction = PlayerErrorFlags.CustomError << 0;
}

// ... inside a NetworkBehaviour
[ServerRpc]
void CmdDoSomething(int data)
{
    // The IsActionValid method would contain your custom validation logic.
    if (!IsActionValid(data))
    {
        // Penalize the player with a moderate cost for sending invalid data.
        Owner.SetError(10, MyErrorFlags.InvalidAction);
        return;
    }

    // ... process valid data
}
```

### Critical Error Example

For severe violations, use `PlayerErrorFlags.Critical` with a high cost to trigger the handler instantly.

```csharp
[ServerRpc]
void CmdTryAdminAction(string command)
{
    // The IsAdmin method would check if the player has admin privileges.
    if (!IsAdmin(Owner))
    {
        // A non-admin tried to use an admin command.
        // Set cost higher than MaxTokens (default 200) to trigger the limit immediately.
        Owner.SetError(10000, PlayerErrorFlags.Critical);
        return;
    }

    // ... execute admin command
}
```

### ServerRpc Without Authority (with Sender)

Sometimes you need a `ServerRpc` to be callable from any client, not just the owner of the `NetworkBehaviour`, and you need to know which client sent the RPC. Use `requireAuthority = false` and include `INetworkPlayer sender = null` as a parameter.

```csharp
[Client]
public void SendPublicMessage(string message) 
{
    // client side check before sending message
    if (string.IsNullOrWhiteSpace(message) || message.Length > 100)
        return;

    CmdSendPublicMessage(message)
}

[ServerRpc(requireAuthority = false)]
void CmdSendPublicMessage(string message, INetworkPlayer sender = null)
{
    if (string.IsNullOrWhiteSpace(message) || message.Length > 100)
    {
        // Invalid message length. this is very likely a cheat because message length is checked on client before
        // how ever this is just chat message nothing not critical gameplay
        // for example could be from chat mod with higher size that they left on after playing on a modded server
        sender.SetError(50, PlayerErrorFlags.LikelyCheater); 
        return;
    }

    if (CheckMessageRateLimit(sender)) 
    {
        // player sent more message than chat rate limit, just use low cost
        sender.SetError(1, PlayerErrorFlags.None); 
        return;
    }

    // ...
}
```

## Custom Error Handling

Instead of the default disconnect, you can define a custom callback to execute when a player reaches their error limit using `NetworkServer.SetErrorRateLimitReachedCallback`.

This callback is best used alongside `NetworkAuthenticator` so that you can ban or timeout users, stopping them from reconnecting.

You can check `player.ErrorFlags` to see how important the errors have been.

```csharp
using Mirage;
using UnityEngine;

public class MyGameServer : MonoBehaviour
{
    public NetworkServer server;

    void Start()
    {
        server.SetErrorRateLimitReachedCallback(OnPlayerErrorLimitReached);
    }

    void OnPlayerErrorLimitReached(INetworkPlayer player)
    {
        Debug.LogWarning($"Player {player} reached error limit with flags: {player.ErrorFlags}");

        if ((player.ErrorFlags & PlayerErrorFlags.Critical) != 0)
        {
            // For critical errors, always disconnect.
            player.Disconnect();
            
            // ... add player to ban or timeout list here so they can't reconnect
            
            return;
        }
        else if ((player.ErrorFlags & MyErrorFlags.InvalidAction) != 0)
        {
            // For our custom action, maybe just send a warning.
            // Note: You would need to implement the ChatMessage struct and its handler.
            // player.Send(new ChatMessage("You are performing too many invalid actions."));
        }
        // ... other custom logic

        // Reset flags after handling
        player.ResetErrorFlag();
    }
}
```
