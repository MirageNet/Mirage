---
id: SyncStack-1
title: SyncStack<T>
---

# Class SyncStack&lt;T&gt;



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
public class SyncStack<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISyncObject
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### SyncStack()



##### Declaration

```cs
public SyncStack()
```

#### SyncStack(Stack&lt;T&gt;)



##### Declaration

```cs
public SyncStack(Stack<T> objects)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.Stack&lt;T&gt; | objects |  |

### Properties

#### Count

##### Declaration

```cs
public int Count { get; }
```
#### IsReadOnly

##### Declaration

```cs
public bool IsReadOnly { get; }
```
#### IsDirty

##### Declaration

```cs
public bool IsDirty { get; }
```
### Methods
#### ISyncObject.SetShouldSyncFrom(Boolean)



##### Declaration

```cs
void ISyncObject.SetShouldSyncFrom(bool shouldSync)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Boolean | shouldSync |  |


#### ISyncObject.SetNetworkBehaviour(NetworkBehaviour)



##### Declaration

```cs
void ISyncObject.SetNetworkBehaviour(NetworkBehaviour networkBehaviour)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.NetworkBehaviour | networkBehaviour |  |


#### Flush()



##### Declaration

```cs
public void Flush()
```


#### Reset()



##### Declaration

```cs
public void Reset()
```


#### OnSerializeAll(NetworkWriter)



##### Declaration

```cs
public void OnSerializeAll(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |


#### OnSerializeDelta(NetworkWriter)



##### Declaration

```cs
public void OnSerializeDelta(NetworkWriter writer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |


#### OnDeserializeAll(NetworkReader)



##### Declaration

```cs
public void OnDeserializeAll(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |


#### OnDeserializeDelta(NetworkReader)



##### Declaration

```cs
public void OnDeserializeDelta(NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |


#### Push(T)



##### Declaration

```cs
public void Push(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |


#### AddRange(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void AddRange(IEnumerable<T> range)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | range |  |


#### Clear()



##### Declaration

```cs
public void Clear()
```


#### CopyTo(T[], Int32)



##### Declaration

```cs
public void CopyTo(T[] array, int arrayIndex)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| {T}[] | array |  |
| System.Int32 | arrayIndex |  |


#### Pop()



##### Declaration

```cs
public T Pop()
```

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### IEnumerable&lt;T&gt;.GetEnumerator()



##### Declaration

```cs
IEnumerator<T> IEnumerable<T>.GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.IEnumerator&lt;T&gt; |  |

#### IEnumerable.GetEnumerator()



##### Declaration

```cs
IEnumerator IEnumerable.GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.IEnumerator |  |

