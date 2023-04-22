# Ready Check and Lobby Ready

:::info
See the API reference for more details
[ReadCheck](/docs/reference/Mirage.Components/ReadCheck)
[LobbyReady](/docs/reference/Mirage.Components/LobbyReady)
:::

### Setup

1. Attach the `LobbyReady` component to your network manager or other non-networked object.
2. Attach the `ReadyCheck` component to your player's lobby object.
3. Set the sync direction on `ReadyCheck` to `From Server and Owner`, `To Server`, `Owner`, and optionally `Observers`. This will allow the value to be set on the owner client as well as on the server to reset it when, like when `LobbyReady.SetAllClientsNotReady()` is called.

### Usage

#### Setting Player Ready

To set a player as ready, you can simply update the `IsReady` field of their `ReadyCheck` component to true. This can be done either manually through code, or through user input such as a "Ready" button. Mirage will then sync this change to server and other clients. For example:

{{{ Path:'Snippets/LobbyReadyCheck.cs' Name:'set-ready' }}}


#### Reacting to Ready changes

When the `IsReady` field of a player's `ReadyCheck` component is changed, the `OnReadyChanged` event is invoked on all clients to reflect the new value. You can subscribe to this event and perform actions based on the player's ready state. For example, you can update UI elements to show the player's current ready status:

{{{ Path:'Snippets/LobbyReadyCheck.cs' Name:'ready-ui' }}}


#### Sending Messages to Ready Players

To send a message to all players that are ready, you can use the `LobbyReady.SendToReady` function. Here's an example:

{{{ Path:'Snippets/LobbyReadyCheck.cs' Name:'send-to-ready' }}}

You can also send messages to not ready players by setting the `sendToReady` parameter to false. Note that this function only sends messages to players that have `ReadyCheck` attached to their character and are synced with the server.

{{{ Path:'Snippets/LobbyReadyCheck.cs' Name:'send-to-not-ready' }}}


#### Resetting Ready

Resetting Ready State for All Players
You can reset the `IsReady` field for all players by calling `LobbyReady.SetAllClientsNotReady()`. Here's an example:

{{{ Path:'Snippets/LobbyReadyCheck.cs' Name:'set-all-not-ready' }}}

This will set the `IsReady` field to `false` for all `ReadyCheck` on the server, the values will then be synced to client.
