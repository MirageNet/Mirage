---
id: Peer
title: Peer
---

# Class Peer


Controls flow of data in/out of mirage, Uses 



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
public sealed class Peer : IPeer
```

### Constructors

#### Peer(ISocket, Int32, IDataHandler, Config, ILogger, Metrics)



##### Declaration

```cs
public Peer(ISocket socket, int maxPacketSize, IDataHandler dataHandler, Config config = null, ILogger logger = null, Metrics metrics = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.ISocket | socket |  |
| System.Int32 | maxPacketSize |  |
| Mirage.SocketLayer.IDataHandler | dataHandler |  |
| Mirage.SocketLayer.Config | config |  |
| ILogger | logger |  |
| Mirage.SocketLayer.Metrics | metrics |  |

### Properties

#### PoolMetrics

##### Declaration

```cs
public PoolMetrics PoolMetrics { get; }
```
### Methods
#### Bind(IBindEndPoint)



##### Declaration

```cs
public void Bind(IBindEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IBindEndPoint | endPoint |  |


#### Connect(IConnectEndPoint)



##### Declaration

```cs
public IConnection Connect(IConnectEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectEndPoint | endPoint |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnection |  |

#### Close()



##### Declaration

```cs
public void Close()
```


#### UpdateSent()


Call this at end of frame to send new batches



##### Declaration

```cs
public void UpdateSent()
```


#### UpdateReceive()


Call this at the start of the frame to receive new messages



##### Declaration

```cs
public void UpdateReceive()
```


#### GetMaxUnreliableMessageSize()



##### Declaration

```cs
public int GetMaxUnreliableMessageSize()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### GetMaxNotifyMessageSize()



##### Declaration

```cs
public int GetMaxNotifyMessageSize()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### GetMaxReliableMessageSize()



##### Declaration

```cs
public int GetMaxReliableMessageSize()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

