---
id: SyncDictionary-2
title: SyncDictionary<TKey, TValue>
---

# Class SyncDictionary&lt;TKey, TValue&gt;



<div class="inheritance">

##### Inheritance

<div class="level" style={{"--data-index": 0}}>
System.Object
</div>
<div class="level" style={{"--data-index": 1}}>
Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;
</div>
</div>

##### Inherited Members

<details>
<summary>Show</summary>

Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.objects


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Count


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.IsReadOnly


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Mirage.Collections.ISyncObject.SetShouldSyncFrom(System.Boolean)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Mirage.Collections.ISyncObject.SetNetworkBehaviour(Mirage.NetworkBehaviour)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnInsert


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnClear


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnRemove


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnSet


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnChange


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Reset()


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.IsDirty


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Keys


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Values


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.System.Collections.Generic.IReadOnlyDictionary&lt;TKey, TValue&gt;.Keys


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.System.Collections.Generic.IReadOnlyDictionary&lt;TKey, TValue&gt;.Values


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Flush()


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnSerializeAll(Mirage.Serialization.NetworkWriter)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnSerializeDelta(Mirage.Serialization.NetworkWriter)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnDeserializeAll(Mirage.Serialization.NetworkReader)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.OnDeserializeDelta(Mirage.Serialization.NetworkReader)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Clear()


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.ContainsKey(TKey)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Remove(TKey)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Item[TKey]


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.TryGetValue(TKey, TValue)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Add(TKey, TValue)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Add(System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Contains(System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.CopyTo(System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;[], System.Int32)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.Remove(System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;)


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.GetEnumerator()


Mirage.Collections.SyncIDictionary&lt;TKey, TValue&gt;.System.Collections.IEnumerable.GetEnumerator()

</details>

##### Syntax

```cs
public class SyncDictionary<TKey, TValue> : SyncIDictionary<TKey, TValue>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, ISyncObject, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| TKey |  |
| TValue |  |

### Constructors

#### SyncDictionary()



##### Declaration

```cs
public SyncDictionary()
```

#### SyncDictionary(IEqualityComparer&lt;TKey&gt;)



##### Declaration

```cs
public SyncDictionary(IEqualityComparer<TKey> eq)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IEqualityComparer&lt;TKey&gt; | eq |  |

### Properties

#### Values

##### Declaration

```cs
public Dictionary<TKey, TValue>.ValueCollection Values { get; }
```
#### Keys

##### Declaration

```cs
public Dictionary<TKey, TValue>.KeyCollection Keys { get; }
```
### Methods
#### GetEnumerator()



##### Declaration

```cs
public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.Dictionary.Enumerator&lt;&gt; |  |

