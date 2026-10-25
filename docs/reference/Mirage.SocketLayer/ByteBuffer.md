---
id: ByteBuffer
title: ByteBuffer
---

# Class ByteBuffer


Warpper around a byte[] that belongs to a 



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
public sealed class ByteBuffer : IDisposable
```


### Fields

#### array

##### Declaration

```cs
public readonly byte[] array
```
### Methods
#### CreateNew(Int32, Pool&lt;ByteBuffer&gt;)



##### Declaration

```cs
public static ByteBuffer CreateNew(int bufferSize, Pool<ByteBuffer> pool)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bufferSize |  |
| Mirage.SocketLayer.Pool&lt;Mirage.SocketLayer.ByteBuffer&gt; | pool |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.ByteBuffer |  |

#### Release()



##### Declaration

```cs
public void Release()
```


#### IDisposable.Dispose()



##### Declaration

```cs
void IDisposable.Dispose()
```


