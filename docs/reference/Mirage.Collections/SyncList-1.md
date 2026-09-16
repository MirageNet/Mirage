---
id: SyncList-1
title: SyncList<T>
---

# Class SyncList&lt;T&gt;



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
public class SyncList<T> : IList<T>, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, ISyncObject
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### SyncList()



##### Declaration

```cs
public SyncList()
```

#### SyncList(IEqualityComparer&lt;T&gt;)



##### Declaration

```cs
public SyncList(IEqualityComparer<T> comparer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEqualityComparer&lt;T&gt; | comparer |  |

#### SyncList(IList&lt;T&gt;, IEqualityComparer&lt;T&gt;)



##### Declaration

```cs
public SyncList(IList<T> objects, IEqualityComparer<T> comparer = null)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IList&lt;T&gt; | objects |  |
| System.Collections.Generic.IEqualityComparer&lt;T&gt; | comparer |  |

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
#### Item[Int32]

##### Declaration

```cs
public T this[int i] { get; set; }
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


#### Add(T)



##### Declaration

```cs
public void Add(T item)
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


#### IndexOf(T)



##### Declaration

```cs
public int IndexOf(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### FindIndex(Predicate&lt;T&gt;)



##### Declaration

```cs
public int FindIndex(Predicate<T> match)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Predicate&lt;T&gt; | match |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### Find(Predicate&lt;T&gt;)



##### Declaration

```cs
public T Find(Predicate<T> match)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Predicate&lt;T&gt; | match |  |

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### FindAll(Predicate&lt;T&gt;)



##### Declaration

```cs
public List<T> FindAll(Predicate<T> match)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Predicate&lt;T&gt; | match |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.List&lt;T&gt; |  |

#### Insert(Int32, T)



##### Declaration

```cs
public void Insert(int index, T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | index |  |
| T | item |  |


#### InsertRange(Int32, IEnumerable&lt;T&gt;)



##### Declaration

```cs
public void InsertRange(int index, IEnumerable<T> range)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | index |  |
| System.Collections.Generic.IEnumerable&lt;T&gt; | range |  |


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

#### RemoveAt(Int32)



##### Declaration

```cs
public void RemoveAt(int index)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | index |  |


#### RemoveAll(Predicate&lt;T&gt;)



##### Declaration

```cs
public int RemoveAll(Predicate<T> match)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Predicate&lt;T&gt; | match |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### SetItemDirty(T)


Can be used to set item dirty manually.
should be used with classes to avoid having to clear field first
Will invoke OnSet



##### Declaration

```cs
public void SetItemDirty(T item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| T | item |  |


#### SetItemDirtyAt(Int32)


Can be used to set item dirty manually.
should be used with classes to avoid having to clear field first



##### Declaration

```cs
public void SetItemDirtyAt(int index)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Int32 | index |  |


#### GetEnumerator()



##### Declaration

```cs
public SyncList<T>.Enumerator GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Collections.SyncList.Enumerator&lt;&gt; |  |

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

