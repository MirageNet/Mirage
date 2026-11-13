---
id: IMessageReceiver
title: IMessageReceiver
---

# Interface IMessageReceiver


An object that can receive messages




##### Syntax

```cs
public interface IMessageReceiver
```

### Methods
#### RegisterHandler&lt;T&gt;(MessageDelegateWithPlayer&lt;T&gt;, Boolean)


Registers a handler for a network message that has INetworkPlayer and T Message parameters

When network message are sent, the first 2 bytes are the Id for the type T.
When message is received the handler with the matching Id is found and invoked




##### Declaration

```cs
void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler, bool allowUnauthenticated)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.MessageDelegateWithPlayer&lt;T&gt; | handler |  |
| System.Boolean | allowUnauthenticated |  |


#### UnregisterHandler&lt;T&gt;()



##### Declaration

```cs
void UnregisterHandler<T>()
```


#### ClearHandlers()



##### Declaration

```cs
void ClearHandlers()
```


#### HandleMessage(INetworkPlayer, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
void HandleMessage(INetworkPlayer player, ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |


