---
id: SocketFactory
title: SocketFactory
---

# Class SocketFactory


Creates an instance of 



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
</div>

##### Syntax

```cs
public abstract class SocketFactory : MonoBehaviour
```


### Properties

#### MaxPacketSize
Max size for packets sent to or received from Socket
Called once when Sockets are created

##### Declaration

```cs
public abstract int MaxPacketSize { get; }
```
#### IsSupported

Can be used on client (or server) to check if this Socket is supported on the current platform


##### Declaration

```cs
public abstract bool IsSupported { get; }
```
### Methods
#### CreateServerSocket()

Creates a  to be used by  on the server


##### Declaration

```cs
public abstract ISocket CreateServerSocket()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.ISocket |  |

#### GetBindEndPoint()

Creates the  that the Server Socket will bind to


##### Declaration

```cs
public abstract IBindEndPoint GetBindEndPoint()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IBindEndPoint |  |

#### CreateClientSocket()

Creates a  to be used by  on the client


##### Declaration

```cs
public abstract ISocket CreateClientSocket()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.ISocket |  |

#### GetConnectEndPoint(String, Nullable&lt;UInt16&gt;)

Creates the  that the Client Socket will connect to using the parameter given


##### Declaration

```cs
public abstract IConnectEndPoint GetConnectEndPoint(string address = null, ushort? port = default(ushort? ))
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | address |  |
| System.Nullable&lt;System.UInt16&gt; | port |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectEndPoint |  |

