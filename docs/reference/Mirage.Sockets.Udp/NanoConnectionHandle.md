---
id: NanoConnectionHandle
title: NanoConnectionHandle
---

# Class NanoConnectionHandle



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
public sealed class NanoConnectionHandle : IConnectionHandle, IBindEndPoint, IConnectEndPoint, IEquatable<NanoConnectionHandle>
```

### Constructors

#### NanoConnectionHandle(String, UInt16)



##### Declaration

```cs
public NanoConnectionHandle(string host, ushort port)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | host |  |
| System.UInt16 | port |  |

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
#### IConnectionHandle.Disconnect(String)



##### Declaration

```cs
void IConnectionHandle.Disconnect(string gracefulDisconnectReason)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.String | gracefulDisconnectReason |  |


#### CreateCopy()



##### Declaration

```cs
public IConnectionHandle CreateCopy()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IConnectionHandle |  |

#### Equals(NanoConnectionHandle)



##### Declaration

```cs
public bool Equals(NanoConnectionHandle other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Sockets.Udp.NanoConnectionHandle | other |  |

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

