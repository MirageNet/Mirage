---
id: MessageWaiter-1
title: MessageWaiter<T>
---

# Class MessageWaiter&lt;T&gt;


Register handler just for 1 message
Useful on client when you want too receive a single auth message



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
public class MessageWaiter<T>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### MessageWaiter(NetworkClient, Boolean)



##### Declaration

```cs
public MessageWaiter(NetworkClient client, bool allowUnauthenticated = false)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkClient | client |  |
| System.Boolean | allowUnauthenticated |  |
### Methods
#### WaitAsync()



##### Declaration

```cs
public UniTask<(bool disconnected, T message)> WaitAsync()
```

##### Returns
| Type | Description |
| ---- | ---- |
| UniTask&lt;System.ValueTuple&lt;System.Boolean, T&gt;&gt; |  |

#### Callback(MessageDelegateWithPlayer&lt;T&gt;)


Use callback instead of async for methods that uses ArraySegment, because internal buffer will be recylced and data will be load before Async completes



##### Declaration

```cs
public void Callback(MessageDelegateWithPlayer<T> callback)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.MessageDelegateWithPlayer&lt;T&gt; | callback |  |


