---
id: SyncSet-1
title: SyncSet<T>
---

# Class SyncSet&lt;T&gt;



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
public class SyncSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ISyncObject
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### SyncSet(ISet&lt;T&gt;)



##### Declaration

```cs
public SyncSet(ISet<T> objects)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.ISet&lt;T&gt; | objects |  |

### Fields

#### objects

##### Declaration

```cs
protected readonly ISet<T> objects
```

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


#### Reset()



##### Declaration

```cs
public void Reset()
```


#### Flush()



##### Declaration

```cs
public void Flush()
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


#### Add(T)



##### Declaration

```cs
public bool Add(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### ICollection&lt;T&gt;.Add(T)



##### Declaration

```cs
void ICollection<T>.Add(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |


#### Clear()



##### Declaration

```cs
public void Clear()
```


#### Contains(T)



##### Declaration

```cs
public bool Contains(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

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


#### Remove(T)



##### Declaration

```cs
public bool Remove(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### GetEnumerator()



##### Declaration

```cs
public IEnumerator<T> GetEnumerator()
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

#### ExceptWith(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void ExceptWith(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |


#### IntersectWith(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void IntersectWith(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |


#### IsProperSubsetOf(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool IsProperSubsetOf(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsProperSupersetOf(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool IsProperSupersetOf(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsSubsetOf(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool IsSubsetOf(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### IsSupersetOf(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool IsSupersetOf(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Overlaps(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool Overlaps(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### SetEquals(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public bool SetEquals(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### SymmetricExceptWith(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void SymmetricExceptWith(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |


#### UnionWith(IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void UnionWith(IEnumerable<T> other)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEnumerable&lt;T&gt; | other |  |


