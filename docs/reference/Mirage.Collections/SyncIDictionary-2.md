---
id: SyncIDictionary-2
title: SyncIDictionary<TKey, TValue>
---

# Class SyncIDictionary&lt;TKey, TValue&gt;



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
public class SyncIDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, ISyncObject, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
```

##### Type Parameters
| Name | Description |
| ---- | ---- |
| TKey |  |
| TValue |  |

### Constructors

#### SyncIDictionary(IDictionary&lt;TKey, TValue&gt;)



##### Declaration

```cs
public SyncIDictionary(IDictionary<TKey, TValue> objects)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.IDictionary&lt;TKey, TValue&gt; | objects |  |

### Fields

#### objects

##### Declaration

```cs
protected readonly IDictionary<TKey, TValue> objects
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
#### Keys

##### Declaration

```cs
public ICollection<TKey> Keys { get; }
```
#### Values

##### Declaration

```cs
public ICollection<TValue> Values { get; }
```
#### IReadOnlyDictionary&lt;TKey, TValue&gt;.Keys

##### Declaration

```cs
IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys { get; }
```
#### IReadOnlyDictionary&lt;TKey, TValue&gt;.Values

##### Declaration

```cs
IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values { get; }
```
#### Item[TKey]

##### Declaration

```cs
public TValue this[TKey i] { get; set; }
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


#### Clear()



##### Declaration

```cs
public void Clear()
```


#### ContainsKey(TKey)



##### Declaration

```cs
public bool ContainsKey(TKey key)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| TKey | key |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Remove(TKey)



##### Declaration

```cs
public bool Remove(TKey key)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| TKey | key |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### TryGetValue(TKey, out TValue)



##### Declaration

```cs
public bool TryGetValue(TKey key, out TValue value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| TKey | key |  |
| TValue | value |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### Add(TKey, TValue)



##### Declaration

```cs
public void Add(TKey key, TValue value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| TKey | key |  |
| TValue | value |  |


#### Add(KeyValuePair&lt;TKey, TValue&gt;)



##### Declaration

```cs
public void Add(KeyValuePair<TKey, TValue> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt; | item |  |


#### Contains(KeyValuePair&lt;TKey, TValue&gt;)



##### Declaration

```cs
public bool Contains(KeyValuePair<TKey, TValue> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### CopyTo(KeyValuePair&lt;TKey, TValue&gt;[], Int32)



##### Declaration

```cs
public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.KeyValuePair{{TKey},{TValue}}[] | array |  |
| System.Int32 | arrayIndex |  |


#### Remove(KeyValuePair&lt;TKey, TValue&gt;)



##### Declaration

```cs
public bool Remove(KeyValuePair<TKey, TValue> item)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt; | item |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean |  |

#### GetEnumerator()



##### Declaration

```cs
public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.IEnumerator&lt;System.Collections.Generic.KeyValuePair&lt;TKey, TValue&gt;&gt; |  |

#### IEnumerable.GetEnumerator()



##### Declaration

```cs
IEnumerator IEnumerable.GetEnumerator()
```

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.IEnumerator |  |

