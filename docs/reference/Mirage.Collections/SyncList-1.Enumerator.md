---
id: SyncList-1.Enumerator
title: SyncList<T>.Enumerator
---

# Struct SyncList&lt;T&gt;.Enumerator




##### Syntax

```cs
public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
```

### Constructors

#### Enumerator(SyncList&lt;T&gt;)



##### Declaration

```cs
public Enumerator(SyncList<T> list)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Collections.SyncList&lt;T&gt; | list |  |

### Properties

#### Current

##### Declaration

```cs
public T Current { get; }
```
#### IEnumerator.Current

##### Declaration

```cs
object IEnumerator.Current { get; }
```
### Methods
#### MoveNext()



##### Declaration

```cs
public bool MoveNext()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Reset()



##### Declaration

```cs
public void Reset()
```


#### Dispose()



##### Declaration

```cs
public void Dispose()
```


