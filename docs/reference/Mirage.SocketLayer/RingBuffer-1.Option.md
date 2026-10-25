---
id: RingBuffer-1.Option
title: RingBuffer<T>.Option
---

# Struct RingBuffer&lt;T&gt;.Option




##### Syntax

```cs
public struct Option
```


### Fields

#### HasValue

##### Declaration

```cs
public readonly bool HasValue
```
#### Value

##### Declaration

```cs
public readonly T Value
```
### Methods
#### Some(T)



##### Declaration

```cs
public static RingBuffer<T>.Option Some(T value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.RingBuffer.Option&lt;&gt; |  |

#### None()



##### Declaration

```cs
public static RingBuffer<T>.Option None()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.SocketLayer.RingBuffer.Option&lt;&gt; |  |

