---
id: StringStoreBrotliEncoder
title: StringStoreBrotliEncoder
---

# Class StringStoreBrotliEncoder

Advanced write/read methods that use "> to compress strings inside 


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
public class StringStoreBrotliEncoder
```

### Methods
#### GetByteLength()



##### Declaration

```cs
public (int lengthsByteCount, int stringsByteCount) GetByteLength()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.ValueTuple{System.Int32,System.Int32} |  |

#### Send(INetworkPlayer)



##### Declaration

```cs
public void Send(INetworkPlayer player)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.INetworkPlayer | player |  |


#### Send(List&lt;INetworkPlayer&gt;)



##### Declaration

```cs
public void Send(List<INetworkPlayer> players)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.List&lt;Mirage.INetworkPlayer&gt; | players |  |


#### Encode(StringStore, Nullable&lt;Int32&gt;)


Encodes the StringStore so it is ready to send



##### Declaration

```cs
public static StringStoreBrotliEncoder Encode(StringStore stringStore, int? _maxMessageSize = default(int? ))
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.StringStore | stringStore |  |
| System.Nullable&lt;System.Int32&gt; | _maxMessageSize |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.BrotliCompression.StringStoreBrotliEncoder |  |

