---
id: SyncSortedSet-1
title: SyncSortedSet<T>
---

# Class SyncSortedSet&lt;T&gt;



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Collections.SyncSet&lt;T&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Collections.SyncSet&lt;T&gt;.objects


Mirage.Collections.SyncSet&lt;T&gt;.Count


Mirage.Collections.SyncSet&lt;T&gt;.IsReadOnly


Mirage.Collections.SyncSet&lt;T&gt;.Mirage.Collections.ISyncObject.SetShouldSyncFrom(System.Boolean)


Mirage.Collections.SyncSet&lt;T&gt;.Mirage.Collections.ISyncObject.SetNetworkBehaviour(Mirage.NetworkBehaviour)


Mirage.Collections.SyncSet&lt;T&gt;.OnAdd


Mirage.Collections.SyncSet&lt;T&gt;.OnClear


Mirage.Collections.SyncSet&lt;T&gt;.OnRemove


Mirage.Collections.SyncSet&lt;T&gt;.OnChange


Mirage.Collections.SyncSet&lt;T&gt;.Reset()


Mirage.Collections.SyncSet&lt;T&gt;.IsDirty


Mirage.Collections.SyncSet&lt;T&gt;.Flush()


Mirage.Collections.SyncSet&lt;T&gt;.OnSerializeAll(Mirage.Serialization.NetworkWriter)


Mirage.Collections.SyncSet&lt;T&gt;.OnSerializeDelta(Mirage.Serialization.NetworkWriter)


Mirage.Collections.SyncSet&lt;T&gt;.OnDeserializeAll(Mirage.Serialization.NetworkReader)


Mirage.Collections.SyncSet&lt;T&gt;.OnDeserializeDelta(Mirage.Serialization.NetworkReader)


Mirage.Collections.SyncSet&lt;T&gt;.Add(T)


Mirage.Collections.SyncSet&lt;T&gt;.System.Collections.Generic.ICollection&lt;T&gt;.Add(T)


Mirage.Collections.SyncSet&lt;T&gt;.Clear()


Mirage.Collections.SyncSet&lt;T&gt;.Contains(T)


Mirage.Collections.SyncSet&lt;T&gt;.CopyTo(T[], System.Int32)


Mirage.Collections.SyncSet&lt;T&gt;.Remove(T)


Mirage.Collections.SyncSet&lt;T&gt;.System.Collections.IEnumerable.GetEnumerator()


Mirage.Collections.SyncSet&lt;T&gt;.ExceptWith(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.IntersectWith(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.IsProperSubsetOf(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.IsProperSupersetOf(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.IsSubsetOf(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.IsSupersetOf(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.Overlaps(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.SetEquals(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.SymmetricExceptWith(System.Collections.Generic.IEnumerable&lt;T&gt;)


Mirage.Collections.SyncSet&lt;T&gt;.UnionWith(System.Collections.Generic.IEnumerable&lt;T&gt;)

</details>

##### Syntax

```cs
public class SyncSortedSet<T> : SyncSet<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ISyncObject
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| T |  |

### Constructors

#### SyncSortedSet()



##### Declaration

```cs
public SyncSortedSet()
```

#### SyncSortedSet(IComparer&lt;T&gt;)



##### Declaration

```cs
public SyncSortedSet(IComparer<T> comparer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IComparer&lt;T&gt; | comparer |  |
### Methods
#### GetEnumerator()



##### Declaration

```cs
public SortedSet<T>.Enumerator GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.SortedSet.Enumerator&lt;&gt; |  |

