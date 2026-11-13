---
sidebar_position: 14
---
# Ready Check and Lobby Ready

:::info
See the API reference for more details
[ReadCheck](/docs/reference/Mirage.Components/ReadyCheck)
[LobbyReady](/docs/reference/Mirage.Components/LobbyReady)
:::

### Setup

1. Attach the `LobbyReady` component to your network manager or other non-networked object.
2. Attach the `ReadyCheck` component to your player's lobby object.
3. Ensure the `ReadyCheck` component is configured to synchronize its `IsReady` state from the owner client to the server, and from the server to all observers. This is typically handled by the `SyncVar` attributes on the `IsReady` field within the `ReadyCheck` script, which ensures the value is set on the owner client and reset on the server when `LobbyReady.SetAllClientsNotReady()` is called.

### Usage

#### Setting Player Ready

To set a player as ready, you can simply update the `IsReady` field of their `ReadyCheck` component to true. This can be done either manually through code, or through user input such as a "Ready" button. Mirage will then sync this change to server and other clients. For example:

```cs
readyCheck.SetReady(true);
```


#### Reacting to Ready changes

When the `IsReady` field of a player's `ReadyCheck` component is changed, the `OnReadyChanged` event is invoked on all clients to reflect the new value. You can subscribe to this event and perform actions based on the player's ready state. For example, you can update UI elements to show the player's current ready status:

```cs
public class ReadyUI : MonoBehaviour
 {
     public ReadyCheck ReadyCheck;
     public Image Image;

     public void Start()
     {
         ReadyCheck.OnReadyChanged += OnReadyChanged;
         // invoke right away to set the current value
         OnReadyChanged(ReadyCheck.IsReady);
     }

     private void OnReadyChanged(bool ready)
     {
         Image.color = ready ? Color.green : Color.red;
     }
 }
```


#### Sending Messages to Ready Players

To send a message to all players that are ready, you can use the `LobbyReady.SendToReady` function. Here's an example:

```cs
[NetworkMessage]
 // Make sure to regieter message on client
 public struct MyMessage
 {
     public string message;
 }

 public class LobbyController : MonoBehaviour
 {
     public LobbyReady LobbyReady;

     public void SendToReady()
     {
         var myMessage = new MyMessage { message = "Hello, world!" };
         // Send message to ready players
         LobbyReady.SendToReady(myMessage);
     }
 }
```

You can also send messages to not ready players by setting the `sendToReady` parameter to false. Note that this function only sends messages to players that have `ReadyCheck` attached to their character and are synced with the server.

```cs
public void SendToNotReady()
 {
     var myMessage = new MyMessage { message = "Hello, world!" };
     // Send message to ready players
     LobbyReady.SendToReady(myMessage, sendToReady: false);
 }
```


#### Resetting Ready

Resetting Ready State for All Players
You can reset the `IsReady` field for all players by calling `LobbyReady.SetAllClientsNotReady()`. Here's an example:

```cs
public void ClearReady()
 {
     LobbyReady.SetAllClientsNotReady();
 }
```

This will set the `IsReady` field to `false` for all `ReadyCheck` on the server, the values will then be synced to client.
