---
id: Batch
title: Batch
---

# Class Batch



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
public abstract class Batch
```

### Constructors

#### Batch(Int32)



##### Declaration

```cs
public Batch(int maxPacketSize)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | maxPacketSize |  |

### Fields

#### MESSAGE_LENGTH_SIZE

##### Declaration

```cs
public const int MESSAGE_LENGTH_SIZE = 2
```

### Properties

#### Created

##### Declaration

```cs
protected abstract bool Created { get; }
```
### Methods
#### GetBatch()



##### Declaration

```cs
protected abstract byte[] GetBatch()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### GetBatchLength()



##### Declaration

```cs
protected abstract int GetBatchLength()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### CreateNewBatch()



##### Declaration

```cs
protected abstract void CreateNewBatch()
```


#### SendAndReset()



##### Declaration

```cs
protected abstract void SendAndReset()
```


#### AddMessage(Byte[], Int32, Int32)



##### Declaration

```cs
public void AddMessage(byte[] message, int offset, int length)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Byte[] | message |  |
| System.Int32 | offset |  |
| System.Int32 | length |  |


#### Flush()



##### Declaration

```cs
public void Flush()
```


