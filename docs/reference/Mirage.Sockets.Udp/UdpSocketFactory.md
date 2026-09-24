---
id: UdpSocketFactory
title: UdpSocketFactory
---

# Class UdpSocketFactory



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.SocketLayer.SocketFactory
</div>
</div>

##### Syntax

```cs
public sealed class UdpSocketFactory : SocketFactory, IHasAddress, IHasPort
```


### Fields

#### Address

##### Declaration

```cs
public string Address
```
#### Port

##### Declaration

```cs
public ushort Port
```
#### SocketLib

##### Declaration

```cs
public SocketLib SocketLib
```
#### BufferSize

##### Declaration

```cs
public int BufferSize
```

### Properties

#### MaxPacketSize

##### Declaration

```cs
public override int MaxPacketSize { get; }
```
#### IHasAddress.Address

##### Declaration

```cs
string IHasAddress.Address { get; set; }
```
#### IHasPort.Port

##### Declaration

```cs
int IHasPort.Port { get; set; }
```
#### IsSupported

##### Declaration

```cs
public override bool IsSupported { get; }
```
### Methods
#### CreateClientSocket()



##### Declaration

```cs
public override ISocket CreateClientSocket()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.ISocket |  |

#### CreateServerSocket()



##### Declaration

```cs
public override ISocket CreateServerSocket()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.ISocket |  |

#### GetBindEndPoint()



##### Declaration

```cs
public override IBindEndPoint GetBindEndPoint()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.IBindEndPoint |  |

#### GetConnectEndPoint(String, Nullable&lt;UInt16&gt;)



##### Declaration

```cs
public override IConnectEndPoint GetConnectEndPoint(string address = null, ushort? port = default(ushort? ))
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

