---
id: RingBuffer-1
title: RingBuffer<T>
---

# Class RingBuffer&lt;T&gt;



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
public class RingBuffer<T>
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### RingBuffer(Int32, UnityEngine.ILogger)



##### Declaration

```cs
public RingBuffer(int bitCount, UnityEngine.ILogger logger)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bitCount |  |
| UnityEngine.ILogger | logger |  |

### Fields

#### Sequencer

##### Declaration

```cs
public readonly Sequencer Sequencer
```

### Properties

#### Read

##### Declaration

```cs
public uint Read { get; }
```
#### Write

##### Declaration

```cs
public uint Write { get; }
```
#### Count

Number of non-null items in buffer
NOTE: this is not distance from read to write


##### Declaration

```cs
public int Count { get; }
```
#### Capacity

##### Declaration

```cs
public int Capacity { get; }
```
#### IsFull

##### Declaration

```cs
public bool IsFull { get; }
```
### Methods
#### DistanceToRead(UInt32)



##### Declaration

```cs
public long DistanceToRead(uint from)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | from |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int64 |  |

#### Enqueue(T)






##### Declaration

```cs
public uint Enqueue(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt32 | sequence of written item |

#### TryPeak(out T)


Tries to read the item at read index
same as  but does not remove the item after reading it



##### Declaration

```cs
public bool TryPeak(out T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if item exists, or false if it is missing |

#### Exists(UInt32)


Does item exist at index
Index will be moved into bounds



##### Declaration

```cs
public bool Exists(uint index)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | index |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if item exists, or false if it is missing |

#### RemoveNext()


Removes the item at read index and increments read index
can be used after  to do the same as 



##### Declaration

```cs
public void RemoveNext()
```


#### Dequeue()


Removes next item and increments read index
Assumes next items exists, best to use this with 



##### Declaration

```cs
public T Dequeue()
```

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### TryDequeue(out T)


Tries to remove the item at read index



##### Declaration

```cs
public bool TryDequeue(out T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if item exists, or false if it is missing |

#### TryGet(UInt32, out T)



##### Declaration

```cs
public bool TryGet(uint index, out T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | index |  |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### InsertAt(UInt32, T)



##### Declaration

```cs
public void InsertAt(uint index, T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | index |  |
| T | item |  |


#### RemoveAt(UInt32)



##### Declaration

```cs
public void RemoveAt(uint index)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt32 | index |  |


#### MoveReadToNextNonEmpty()


Moves read index to next non empty position
this is useful when removing items from buffer in random order.
Will stop when write == read, or when next buffer item is not empty



##### Declaration

```cs
public void MoveReadToNextNonEmpty()
```


#### MoveReadOne()


Moves read 1 index



##### Declaration

```cs
public void MoveReadOne()
```


#### ClearAndRelease(Action&lt;T&gt;)



##### Declaration

```cs
public void ClearAndRelease(Action<T> releaseItem)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Action&lt;T&gt; | releaseItem |  |


#### Reset()



##### Declaration

```cs
public void Reset()
```


