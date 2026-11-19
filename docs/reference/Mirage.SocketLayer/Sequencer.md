---
id: Sequencer
title: Sequencer
---

# Class Sequencer


A sequence generator that can wrap.
For example a 2 bit sequencer would generate
the following numbers:
    0,1,2,3,0,1,2,3,0,1,2,3...



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
public class Sequencer
```

### Constructors

#### Sequencer(Int32)



##### Declaration

```cs
public Sequencer(int bits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | bits | amount of bits for the sequence |

### Properties

#### Bits

Number of bits used for the sequence generator up to 64


##### Declaration

```cs
public int Bits { get; }
```
### Methods
#### Next()


Generates the next value in the sequence
starts with 0



##### Declaration

```cs
public ulong Next()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 | 0, 1, 2, ..., (2^n)-1, 0, 1, 2, ... |

#### NextAfter(UInt64)


Gets the next sequence value after a given sequence
wraps if necessary



##### Declaration

```cs
public ulong NextAfter(ulong sequence)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | sequence | current sequence value |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 | the next sequence value |

#### MoveInBounds(UInt64)


returns a sequence value from the given value
wraps if necessary



##### Declaration

```cs
public ulong MoveInBounds(ulong sequence)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | sequence | current sequence value |

##### Returns
| Type | Description |
| ---- | ---- |
| System.UInt64 | the next sequence value |

#### Distance(UInt64, UInt64)


Calculates the distance between 2 sequences, taking into account
wrapping



##### Declaration

```cs
public long Distance(ulong from, ulong to)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.UInt64 | from | current sequence value |
| System.UInt64 | to | previous sequence value |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int64 | from - to, adjusted for wrapping |

#### Reset()



##### Declaration

```cs
public void Reset()
```


