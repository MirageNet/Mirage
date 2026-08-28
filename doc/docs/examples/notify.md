---
sidebar_position: 22
---

# Notify Example

The Notify sample is a script-only example, not a playable scene. It shows how to send changes over the unreliable channel, and use **Notify** to find out which messages were delivered, so you only resend the changes the other side doesn't have yet.

This is useful for values that change often (for example a list that grows every update), where reliable messages would add delay but plain unreliable messages could lose data.

## What's Included

The file `NotfyExample.cs` has two classes:

### `SendChangesNotify` (sender)
- Keeps a list of values, and `lastReceivedCount`: how many values the other side is known to have.
- Each `Update` adds a value, then sends a `Changes` message with every value from `lastReceivedCount` onwards. Values that might have been lost are sent again.
- Sends with `player.Send(message, callbacks)`, which uses Notify. The callbacks object implements `INotifyCallBack`:
  - **`OnDelivered`**: the message arrived, so `lastReceivedCount` moves forward and those values aren't sent again.
  - **`OnLost`**: nothing to do, the values will be included in the next message anyway.
- Callback objects are pooled so sending doesn't allocate a new one each time.

```csharp
public void OnDelivered()
{
    // if delivered is newer, then update it
    if (owner.lastReceivedCount < valueCount)
    {
        owner.lastReceivedCount = valueCount;
    }

    // return to pool so it can be re-used
    owner.callbackPool.Push(this);
}
```

### `RecieveChangesNotify` (receiver)
- Registers a handler for the `Changes` message.
- Because the sender may resend values, the receiver works out which values in the message are new and only adds those.
- Messages can arrive late or out of order, so ones with a count the receiver already has are ignored.

## Using It

The classes are plain C#, so you create and drive them from your own code. For example on the server:

1. Create a `SendChangesNotify` for each player (pass the `INetworkPlayer`).
2. Call `Update(value)` whenever you have a new value to send.
3. On the client, create a `RecieveChangesNotify` with `Client.MessageHandler` once the client has started.

:::note
The `Changes` message uses a `List<float>`, which allocates when sending and receiving. The comments in the sample explain how to avoid this for real use.
:::
