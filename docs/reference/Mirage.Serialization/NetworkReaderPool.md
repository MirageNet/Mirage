---
id: NetworkReaderPool
title: NetworkReaderPool
---

# Class NetworkReaderPool


Holds static reference to  of 



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
public static class NetworkReaderPool
```

### Methods
#### Configure(Int32, Int32)



##### Declaration

```cs
public static void Configure(int startPoolSize = 5, int maxPoolSize = 100)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | startPoolSize |  |
| System.Int32 | maxPoolSize |  |


#### GetReader(ArraySegment&lt;Byte&gt;, IObjectLocator)


Gets reader from pool. sets internal array and objectLocator values



##### Declaration

```cs
public static PooledNetworkReader GetReader(ArraySegment<byte> packet, IObjectLocator objectLocator)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; | packet |  |
| Mirage.IObjectLocator | objectLocator | Can be null, but must be set in order to read NetworkIdentity Values |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkReader |  |

#### GetReader(Byte[], IObjectLocator)


Gets reader from pool. sets internal array and objectLocator values



##### Declaration

```cs
public static PooledNetworkReader GetReader(byte[] array, IObjectLocator objectLocator)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |
| Mirage.IObjectLocator | objectLocator | Can be null, but must be set in order to read NetworkIdentity Values |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkReader |  |

#### GetReader(Byte[], Int32, Int32, IObjectLocator)


Gets reader from pool. sets internal array and objectLocator values



##### Declaration

```cs
public static PooledNetworkReader GetReader(byte[] array, int offset, int length, IObjectLocator objectLocator)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | array |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |
| Mirage.IObjectLocator | objectLocator | Can be null, but must be set in order to read NetworkIdentity Values |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.PooledNetworkReader |  |

