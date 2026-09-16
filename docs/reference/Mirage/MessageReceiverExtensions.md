---
id: MessageReceiverExtensions
title: MessageReceiverExtensions
---

# Class MessageReceiverExtensions



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
public static class MessageReceiverExtensions
```

### Methods
#### RegisterHandler&lt;T&gt;(IMessageReceiver, MessageDelegateWithPlayer&lt;T&gt;)


Registers a handler for a network message that has INetworkPlayer and T Message parameters

When network message are sent, the first 2 bytes are the Id for the type T.
When message is received the handler with the matching Id is found and invoked




##### Declaration

```cs
public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateWithPlayer<T> handler)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |
| Mirage.MessageDelegateWithPlayer&lt;T&gt; | handler |  |


#### RegisterHandler&lt;T&gt;(IMessageReceiver, MessageDelegate&lt;T&gt;, Boolean)


Registers a handler for a network message that has just T Message parameter

When network message are sent, the first 2 bytes are the Id for the type T.
When message is received the handler with the matching Id is found and invoked




##### Declaration

```cs
public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegate<T> handler, bool allowUnauthenticated = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |
| Mirage.MessageDelegate&lt;T&gt; | handler |  |
| System.Boolean | allowUnauthenticated |  |


#### RegisterHandler&lt;T&gt;(IMessageReceiver, MessageDelegateWithPlayerAsync&lt;T&gt;, Boolean)


Registers a handler for a network message that has INetworkPlayer and T Message parameters and returns UniTaskVoid.

This allows for async handles without allocations


When network message are sent, the first 2 bytes are the Id for the type T.
When message is received the handler with the matching Id is found and invoked




##### Declaration

```cs
public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateWithPlayerAsync<T> handler, bool allowUnauthenticated = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |
| Mirage.MessageDelegateWithPlayerAsync&lt;T&gt; | handler |  |
| System.Boolean | allowUnauthenticated |  |


#### RegisterHandler&lt;T&gt;(IMessageReceiver, MessageDelegateAsync&lt;T&gt;, Boolean)


Registers a handler for a network message that has just T Message parameter and returns UniTaskVoid.

This allows for async handles without allocations


When network message are sent, the first 2 bytes are the Id for the type T.
When message is received the handler with the matching Id is found and invoked




##### Declaration

```cs
public static void RegisterHandler<T>(this IMessageReceiver receiver, MessageDelegateAsync<T> handler, bool allowUnauthenticated = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.IMessageReceiver | receiver |  |
| Mirage.MessageDelegateAsync&lt;T&gt; | handler |  |
| System.Boolean | allowUnauthenticated |  |


