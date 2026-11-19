---
id: NanoSocket
title: NanoSocket
---

# Class NanoSocket



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
public sealed class NanoSocket : ISocket
```

### Constructors

#### NanoSocket(Int32)



##### Declaration

```cs
public NanoSocket(int bufferSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bufferSize |  |

### Properties

#### Supported

##### Declaration

```cs
public static bool Supported { get; }
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
public IConnectionHandle Connect(IConnectEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectEndPoint | endPoint |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle |  |

#### Close()



##### Declaration

```cs
public void Close()
```


#### Poll()



##### Declaration

```cs
public bool Poll()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Receive(Span&lt;Byte&gt;, out IConnectionHandle)



##### Declaration

```cs
public int Receive(Span<byte> outBuffer, out IConnectionHandle handle)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | outBuffer |  |
| Mirage.SocketLayer.IConnectionHandle | handle |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### Send(IConnectionHandle, ReadOnlySpan&lt;Byte&gt;)



##### Declaration

```cs
public void Send(IConnectionHandle handle, ReadOnlySpan<byte> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle | handle |  |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |


#### ISocket.Tick()



##### Declaration

```cs
void ISocket.Tick()
```


#### ISocket.Flush()



##### Declaration

```cs
void ISocket.Flush()
```


#### ISocket.SetTickEvents(Int32, OnData, OnDisconnect)



##### Declaration

```cs
void ISocket.SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxPacketSize |  |
| Mirage.SocketLayer.OnData | onData |  |
| Mirage.SocketLayer.OnDisconnect | onDisconnect |  |


