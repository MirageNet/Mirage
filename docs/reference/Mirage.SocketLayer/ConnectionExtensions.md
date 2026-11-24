---
id: ConnectionExtensions
title: ConnectionExtensions
---

# Class ConnectionExtensions



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
public static class ConnectionExtensions
```

### Methods
#### SendUnreliable(IConnection, Byte[])



##### Declaration

```cs
public static void SendUnreliable(this IConnection conn, byte[] packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.Byte[] | packet |  |


#### SendUnreliable(IConnection, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public static void SendUnreliable(this IConnection conn, ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |


#### SendNotify(IConnection, Byte[])



##### Declaration

```cs
public static INotifyToken SendNotify(this IConnection conn, byte[] packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.Byte[] | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(IConnection, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public static INotifyToken SendNotify(this IConnection conn, ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.INotifyToken |  |

#### SendNotify(IConnection, Byte[], INotifyCallBack)



##### Declaration

```cs
public static void SendNotify(this IConnection conn, byte[] packet, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.Byte[] | packet |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendNotify(IConnection, ArraySegment&lt;Byte&gt;, INotifyCallBack)



##### Declaration

```cs
public static void SendNotify(this IConnection conn, ArraySegment<byte> packet, INotifyCallBack callBacks)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |
| Mirage.SocketLayer.INotifyCallBack | callBacks |  |


#### SendReliable(IConnection, Byte[])



##### Declaration

```cs
public static void SendReliable(this IConnection conn, byte[] packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.Byte[] | packet |  |


#### SendReliable(IConnection, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public static void SendReliable(this IConnection conn, ArraySegment<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnection | conn |  |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |


