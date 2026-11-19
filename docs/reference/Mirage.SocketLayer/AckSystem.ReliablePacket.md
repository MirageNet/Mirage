---
id: AckSystem.ReliablePacket
title: AckSystem.ReliablePacket
---

# Class AckSystem.ReliablePacket



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
public class ReliablePacket
```


### Fields

#### LastSequence

##### Declaration

```cs
public ushort LastSequence
```
#### Length

##### Declaration

```cs
public int Length
```
#### Buffer

##### Declaration

```cs
public ByteBuffer Buffer
```
#### Order

##### Declaration

```cs
public ushort Order
```
#### Sequences

##### Declaration

```cs
public readonly List<ushort> Sequences
```
### Methods
#### OnSend(UInt16)



##### Declaration

```cs
public void OnSend(ushort sequence)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt16 | sequence |  |


#### OnAck()



##### Declaration

```cs
public void OnAck()
```


#### Setup(UInt16, ByteBuffer, Int32)



##### Declaration

```cs
public void Setup(ushort order, ByteBuffer buffer, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt16 | order |  |
| Mirage.SocketLayer.ByteBuffer | buffer |  |
| System.Int32 | length |  |


#### GetHashCode()



##### Declaration

```cs
public override int GetHashCode()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

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

#### CreateNew(Pool&lt;AckSystem.ReliablePacket&gt;)



##### Declaration

```cs
public static AckSystem.ReliablePacket CreateNew(Pool<AckSystem.ReliablePacket> pool)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.SocketLayer.Pool&lt;Mirage.SocketLayer.AckSystem.ReliablePacket&gt; | pool |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.AckSystem.ReliablePacket |  |

