---
id: UdpConnectionHandle
title: UdpConnectionHandle
---

# Class UdpConnectionHandle



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
public class UdpConnectionHandle : IConnectionHandle, IBindEndPoint, IConnectEndPoint, IEquatable<UdpConnectionHandle>
```

### Constructors

#### UdpConnectionHandle(EndPoint)



##### Declaration

```cs
public UdpConnectionHandle(EndPoint endPoint)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Net.EndPoint | endPoint |  |

### Fields

#### inner

##### Declaration

```cs
public EndPoint inner
```

### Properties

#### IConnectionHandle.IsStateful

##### Declaration

```cs
bool IConnectionHandle.IsStateful { get; }
```
#### IConnectionHandle.SupportsGracefulDisconnect

##### Declaration

```cs
bool IConnectionHandle.SupportsGracefulDisconnect { get; }
```
#### IConnectionHandle.SocketLayerConnection

##### Declaration

```cs
ISocketLayerConnection IConnectionHandle.SocketLayerConnection { get; set; }
```
### Methods
#### Equals(UdpConnectionHandle)



##### Declaration

```cs
public bool Equals(UdpConnectionHandle other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Sockets.Udp.UdpConnectionHandle | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Equals(Object)



##### Declaration

```cs
public override bool Equals(object obj)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Object | obj |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### GetHashCode()



##### Declaration

```cs
public override int GetHashCode()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### ToString()



##### Declaration

```cs
public override string ToString()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### IConnectionHandle.Disconnect(String)



##### Declaration

```cs
void IConnectionHandle.Disconnect(string gracefulDisconnectReason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | gracefulDisconnectReason |  |


#### IConnectionHandle.CreateCopy()



##### Declaration

```cs
IConnectionHandle IConnectionHandle.CreateCopy()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle |  |

