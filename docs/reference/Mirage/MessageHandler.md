---
id: MessageHandler
title: MessageHandler
---

# Class MessageHandler



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>
</details>

##### Syntax

```cs
public class MessageHandler : IMessageReceiver
```

### Constructors

#### MessageHandler(IObjectLocator, Boolean, Boolean)



##### Declaration

```cs
public MessageHandler(IObjectLocator objectLocator, bool disconnectOnException, bool rethrowException = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IObjectLocator | objectLocator |  |
| System.Boolean | disconnectOnException |  |
| System.Boolean | rethrowException |  |
### Methods
#### RegisterHandler&lt;T&gt;(MessageDelegateWithPlayer&lt;T&gt;, Boolean)



##### Declaration

```cs
public void RegisterHandler<T>(MessageDelegateWithPlayer<T> handler, bool allowUnauthenticated)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.MessageDelegateWithPlayer&lt;T&gt; | handler |  |
| System.Boolean | allowUnauthenticated |  |


#### UnregisterHandler&lt;T&gt;()


Unregister a handler for a particular message type.
Note: Messages dont need to be unregister when server or client stops as MessageHandler will be re-created next time server or client starts



##### Declaration

```cs
public void UnregisterHandler<T>()
```


#### ClearHandlers()


Clear all registered callback handlers.



##### Declaration

```cs
public void ClearHandlers()
```


#### HandleMessage(INetworkPlayer, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public void HandleMessage(INetworkPlayer player, ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |


#### HandleExceptionInReader(INetworkPlayer, Exception)



##### Declaration

```cs
public void HandleExceptionInReader(INetworkPlayer player, Exception e)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.Exception | e |  |


#### HandleExceptionInMessage(INetworkPlayer, Exception)



##### Declaration

```cs
public void HandleExceptionInMessage(INetworkPlayer player, Exception e)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |
| System.Exception | e |  |


