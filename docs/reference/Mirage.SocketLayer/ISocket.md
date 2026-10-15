---
id: ISocket
title: ISocket
---

# Interface ISocket


Link between Mirage and the outside world




##### Syntax

```cs
public interface ISocket
```

### Methods
#### Bind(IBindEndPoint)


Starts listens for data on an endPoint
Used by Server to allow clients to connect



##### Declaration

```cs
void Bind(IBindEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IBindEndPoint | endPoint | the endPoint to listen on |


#### Connect(IConnectEndPoint)


Sets up Socket ready to send data to endPoint as a client



##### Declaration

```cs
IConnectionHandle Connect(IConnectEndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectEndPoint | endPoint | the endPoint to connect to |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle | returns the handle for the connection |

#### Close()


Closes the socket, stops receiving messages from other peers



##### Declaration

```cs
void Close()
```


#### SetTickEvents(Int32, OnData, OnDisconnect)


Set events that will be used by . Will be called once when  is set up with .
The onData and onDisconnect events should only be invoked from within the  method.



##### Declaration

```cs
void SetTickEvents(int maxPacketSize, OnData onData, OnDisconnect onDisconnect)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxPacketSize |  |
| Mirage.SocketLayer.OnData | onData |  |
| Mirage.SocketLayer.OnDisconnect | onDisconnect |  |


#### Tick()


Should invoke  and  events for any new data or disconnections.
This method is called by  once per frame.



##### Declaration

```cs
void Tick()
```


#### Flush()


Optional function,  will call this after 



##### Declaration

```cs
void Flush()
```


#### Poll()


Checks if a packet is available 



##### Declaration

```cs
bool Poll()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if there is atleast 1 packet to read |

#### Receive(Span&lt;Byte&gt;, out IConnectionHandle)


Gets next packet
Should be called after Poll

    Implementation should check that incoming packet is within the size of buffer,
    and make sure not to return bytesReceived above that size




##### Declaration

```cs
int Receive(Span<byte> outBuffer, out IConnectionHandle handle)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Span&lt;System.Byte&gt; | outBuffer |  |
| Mirage.SocketLayer.IConnectionHandle | handle |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 | length of packet, should not be above <code data-dev-comment-type="paramref" class="paramref">buffer</code> length |

#### Send(IConnectionHandle, ReadOnlySpan&lt;Byte&gt;)


Sends a packet to an endPoint
Implementation should use length because packet is a buffer than may contain data from previous packets



##### Declaration

```cs
void Send(IConnectionHandle handle, ReadOnlySpan<byte> packet)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle | handle |  |
| ReadOnlySpan&lt;System.Byte&gt; | packet | buffer that contains the packet, starting at index 0 |


