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

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-flags' }}}

*   **`SerializationLimit`**: Triggered when a client sends a payload (like a string, list, or array) whose size exceeds the limit defined by the `[MaxLength]` attribute, throwing a `SerializationLimitException`. This carries a cost of `100` to quickly penalize and disconnect potentially malicious clients.

You can use these flags to identify an error's cause when implementing custom logic. You can also define your own flags using the `CustomError` bit as a starting point:

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-custom-flags' }}}

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

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-custom-error-class' }}}

// ... inside a NetworkBehaviour
{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-custom-error-method' }}}

### Critical Error Example

For severe violations, use `PlayerErrorFlags.Critical` with a high cost to trigger the handler instantly.

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-admin-action' }}}

### ServerRpc Without Authority (with Sender)

Sometimes you need a `ServerRpc` to be callable from any client, not just the owner of the `NetworkBehaviour`, and you need to know which client sent the RPC. Use `requireAuthority = false` and include `INetworkPlayer sender = null` as a parameter.

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-public-message' }}}

## Custom Error Handling

Instead of the default disconnect, you can define a custom callback to execute when a player reaches their error limit using `NetworkServer.SetErrorRateLimitReachedCallback`.

This callback is best used alongside `NetworkAuthenticator` so that you can ban or timeout users, stopping them from reconnecting.

You can check `player.ErrorFlags` to see how important the errors have been.

{{{ Path:'Snippets/General/ErrorHandlingSnippets.cs' Name:'error-handling-custom-handler' }}}
