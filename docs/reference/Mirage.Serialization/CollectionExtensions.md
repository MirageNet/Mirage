---
id: CollectionExtensions
title: CollectionExtensions
---

# Class CollectionExtensions



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
public static class CollectionExtensions
```

### Methods
#### WriteSpanAndSize(NetworkWriter, Span&lt;Byte&gt;)


Write method for weaver to use



##### Declaration

```cs
public static void WriteSpanAndSize(this NetworkWriter writer, Span<byte> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Span&lt;System.Byte&gt; | span |  |


#### WriteSpanAndSize(NetworkWriter, Span&lt;Byte&gt;, Int32)



##### Declaration

```cs
public static void WriteSpanAndSize(this NetworkWriter writer, Span<byte> span, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Span&lt;System.Byte&gt; | span |  |
| System.Int32 | maxLength |  |


#### WriteSpanAndSize(NetworkWriter, ReadOnlySpan&lt;Byte&gt;)


Write method for weaver to use



##### Declaration

```cs
public static void WriteSpanAndSize(this NetworkWriter writer, ReadOnlySpan<byte> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |


#### WriteSpanAndSize(NetworkWriter, ReadOnlySpan&lt;Byte&gt;, Int32)



##### Declaration

```cs
public static void WriteSpanAndSize(this NetworkWriter writer, ReadOnlySpan<byte> span, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| ReadOnlySpan&lt;System.Byte&gt; | span |  |
| System.Int32 | maxLength |  |


#### WriteBytesAndSize(NetworkWriter, Byte[], Int32, Int32)


For byte arrays with dynamic size, where the reader doesn&apos;t know how many will come 



##### Declaration

```cs
public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer, int offset, int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Byte[] | buffer | array or null |
| System.Int32 | offset |  |
| System.Int32 | count |  |


#### WriteBytesAndSize(NetworkWriter, Byte[])


Write method for weaver to use



##### Declaration

```cs
public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Byte[] | buffer | array or null |


#### WriteBytesAndSize(NetworkWriter, Byte[], Int32)



##### Declaration

```cs
public static void WriteBytesAndSize(this NetworkWriter writer, byte[] buffer, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Byte[] | buffer |  |
| System.Int32 | maxLength |  |


#### WriteBytesAndSizeSegment(NetworkWriter, ArraySegment&lt;Byte&gt;)



##### Declaration

```cs
public static void WriteBytesAndSizeSegment(this NetworkWriter writer, ArraySegment<byte> buffer)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.ArraySegment&lt;System.Byte&gt; | buffer |  |


#### WriteBytesAndSizeSegment(NetworkWriter, ArraySegment&lt;Byte&gt;, Int32)



##### Declaration

```cs
public static void WriteBytesAndSizeSegment(this NetworkWriter writer, ArraySegment<byte> buffer, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.ArraySegment&lt;System.Byte&gt; | buffer |  |
| System.Int32 | maxLength |  |


#### WriteList&lt;T&gt;(NetworkWriter, List&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteList<T>(this NetworkWriter writer, List<T> list)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Collections.Generic.List&lt;T&gt; | list |  |


#### WriteList&lt;T&gt;(NetworkWriter, List&lt;T&gt;, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteList<T>(this NetworkWriter writer, List<T> list, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Collections.Generic.List&lt;T&gt; | list |  |
| System.Int32 | maxLength |  |


#### WriteArray&lt;T&gt;(NetworkWriter, T[])



##### Declaration

```cs
public static void WriteArray<T>(this NetworkWriter writer, T[] array)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| {T}[] | array |  |


#### WriteArray&lt;T&gt;(NetworkWriter, T[], Int32)



##### Declaration

```cs
public static void WriteArray<T>(this NetworkWriter writer, T[] array, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| {T}[] | array |  |
| System.Int32 | maxLength |  |


#### WriteArraySegment&lt;T&gt;(NetworkWriter, ArraySegment&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteArraySegment<T>(this NetworkWriter writer, ArraySegment<T> segment)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.ArraySegment&lt;T&gt; | segment |  |


#### WriteArraySegment&lt;T&gt;(NetworkWriter, ArraySegment&lt;T&gt;, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteArraySegment<T>(this NetworkWriter writer, ArraySegment<T> segment, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.ArraySegment&lt;T&gt; | segment |  |
| System.Int32 | maxLength |  |


#### WriteDictionary&lt;TKey, TValue&gt;(NetworkWriter, Dictionary&lt;TKey, TValue&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteDictionary<TKey, TValue>(this NetworkWriter writer, Dictionary<TKey, TValue> dictionary)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; | dictionary |  |


#### WriteDictionary&lt;TKey, TValue&gt;(NetworkWriter, Dictionary&lt;TKey, TValue&gt;, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteDictionary<TKey, TValue>(this NetworkWriter writer, Dictionary<TKey, TValue> dictionary, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; | dictionary |  |
| System.Int32 | maxLength |  |


#### WriteSpan&lt;T&gt;(NetworkWriter, Span&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteSpan<T>(this NetworkWriter writer, Span<T> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Span&lt;T&gt; | span |  |


#### WriteSpan&lt;T&gt;(NetworkWriter, Span&lt;T&gt;, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteSpan<T>(this NetworkWriter writer, Span<T> span, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Span&lt;T&gt; | span |  |
| System.Int32 | maxLength |  |


#### WriteReadOnlySpan&lt;T&gt;(NetworkWriter, ReadOnlySpan&lt;T&gt;)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteReadOnlySpan<T>(this NetworkWriter writer, ReadOnlySpan<T> span)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| ReadOnlySpan&lt;T&gt; | span |  |


#### WriteReadOnlySpan&lt;T&gt;(NetworkWriter, ReadOnlySpan&lt;T&gt;, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static void WriteReadOnlySpan<T>(this NetworkWriter writer, ReadOnlySpan<T> span, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| ReadOnlySpan&lt;T&gt; | span |  |
| System.Int32 | maxLength |  |


#### ReadBytesAndSize(NetworkReader)



##### Declaration

```cs
public static byte[] ReadBytesAndSize(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] | array or null |

#### ReadBytesAndSize(NetworkReader, Int32)



##### Declaration

```cs
public static byte[] ReadBytesAndSize(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### ReadBytesAndSizeSegment(NetworkReader)



##### Declaration

```cs
public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; |  |

#### ReadBytesAndSizeSegment(NetworkReader, Int32)



##### Declaration

```cs
public static ArraySegment<byte> ReadBytesAndSizeSegment(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;System.Byte&gt; |  |

#### ReadSpanAndSize(NetworkReader)


Read method for weaver to use



##### Declaration

```cs
public static Span<byte> ReadSpanAndSize(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Span&lt;System.Byte&gt; |  |

#### ReadSpanAndSize(NetworkReader, Int32)



##### Declaration

```cs
public static Span<byte> ReadSpanAndSize(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Span&lt;System.Byte&gt; |  |

#### ReadReadOnlySpanAndSize(NetworkReader)


Read method for weaver to use



##### Declaration

```cs
public static ReadOnlySpan<byte> ReadReadOnlySpanAndSize(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; |  |

#### ReadReadOnlySpanAndSize(NetworkReader, Int32)



##### Declaration

```cs
public static ReadOnlySpan<byte> ReadReadOnlySpanAndSize(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;System.Byte&gt; |  |

#### ReadBytes(NetworkReader, Int32)



##### Declaration

```cs
public static byte[] ReadBytes(this NetworkReader reader, int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | count |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Byte[] |  |

#### ReadList&lt;T&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static List<T> ReadList<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.List&lt;T&gt; |  |

#### ReadList&lt;T&gt;(NetworkReader, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static List<T> ReadList<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.List&lt;T&gt; |  |

#### ReadArray&lt;T&gt;(NetworkReader)



##### Declaration

```cs
public static T[] ReadArray<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| {T}[] |  |

#### ReadArray&lt;T&gt;(NetworkReader, Int32)



##### Declaration

```cs
public static T[] ReadArray<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| {T}[] |  |

#### ReadArraySegment&lt;T&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static ArraySegment<T> ReadArraySegment<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;T&gt; |  |

#### ReadArraySegment&lt;T&gt;(NetworkReader, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static ArraySegment<T> ReadArraySegment<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.ArraySegment&lt;T&gt; |  |

#### ReadDictionary&lt;TKey, TValue&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; |  |

#### ReadDictionary&lt;TKey, TValue&gt;(NetworkReader, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; |  |

#### ReadSpan&lt;T&gt;(NetworkReader)


Reads a span from the reader.
NOTE: this method allocates a new array internally to hold the data.



##### Declaration

```cs
[WeaverSerializeCollection]
public static Span<T> ReadSpan<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Span&lt;T&gt; | A span pointing to a new array with the read data. |

#### ReadSpan&lt;T&gt;(NetworkReader, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static Span<T> ReadSpan<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Span&lt;T&gt; |  |

#### ReadReadOnlySpan&lt;T&gt;(NetworkReader)



##### Declaration

```cs
[WeaverSerializeCollection]
public static ReadOnlySpan<T> ReadReadOnlySpan<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;T&gt; |  |

#### ReadReadOnlySpan&lt;T&gt;(NetworkReader, Int32)



##### Declaration

```cs
[WeaverSerializeCollection]
public static ReadOnlySpan<T> ReadReadOnlySpan<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| ReadOnlySpan&lt;T&gt; |  |

#### ReadListNonAlloc&lt;T&gt;(NetworkReader, List&lt;T&gt;, out Boolean)


Reads a list from the reader into a provided list so that no new list is allocated.
This will clear the list before adding the new items.



##### Declaration

```cs
public static void ReadListNonAlloc<T>(this NetworkReader reader, List<T> outList, out bool wasNull)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Collections.Generic.List&lt;T&gt; | outList | List to be populated with data. Can not be null. |
| System.Boolean | wasNull | true if the list was null on the wire |


#### ReadListNonAlloc&lt;T&gt;(NetworkReader, List&lt;T&gt;, out Boolean, Int32)



##### Declaration

```cs
public static void ReadListNonAlloc<T>(this NetworkReader reader, List<T> outList, out bool wasNull, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Collections.Generic.List&lt;T&gt; | outList |  |
| System.Boolean | wasNull |  |
| System.Int32 | maxLength |  |


#### ReadDictionaryNonAlloc&lt;TKey, TValue&gt;(NetworkReader, Dictionary&lt;TKey, TValue&gt;, out Boolean)


Reads a dictionary from the reader into a provided dictionary so that no new dictionary is allocated.
This will clear the dictionary before adding the new items.



##### Declaration

```cs
public static void ReadDictionaryNonAlloc<TKey, TValue>(this NetworkReader reader, Dictionary<TKey, TValue> outDictionary, out bool wasNull)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; | outDictionary |  |
| System.Boolean | wasNull |  |


#### ReadDictionaryNonAlloc&lt;TKey, TValue&gt;(NetworkReader, Dictionary&lt;TKey, TValue&gt;, out Boolean, Int32)



##### Declaration

```cs
public static void ReadDictionaryNonAlloc<TKey, TValue>(this NetworkReader reader, Dictionary<TKey, TValue> outDictionary, out bool wasNull, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Collections.Generic.Dictionary&lt;TKey, TValue&gt; | outDictionary |  |
| System.Boolean | wasNull |  |
| System.Int32 | maxLength |  |


#### ReadArrayNonAlloc&lt;T&gt;(NetworkReader, T[])


Reads an array from the reader into a provided array so that no new array is allocated.



##### Declaration

```cs
public static int? ReadArrayNonAlloc<T>(this NetworkReader reader, T[] outArray)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| {T}[] | outArray | Array to be populated with data. Must be large enough to hold all elements. |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Nullable&lt;System.Int32&gt; | The number of elements read, or null if the array was null when sent |

#### ReadArrayNonAlloc&lt;T&gt;(NetworkReader, T[], Int32)



##### Declaration

```cs
public static int? ReadArrayNonAlloc<T>(this NetworkReader reader, T[] outArray, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| {T}[] | outArray |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Nullable&lt;System.Int32&gt; |  |

#### ReadSpanNonAlloc&lt;T&gt;(NetworkReader, Span&lt;T&gt;)


Reads a span from the reader into a provided span so that no new array is allocated.



##### Declaration

```cs
public static int ReadSpanNonAlloc<T>(this NetworkReader reader, Span<T> outSpan)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| Span&lt;T&gt; | outSpan | Span to be populated with data. Must be large enough to hold all elements. |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 | The number of elements read. |

#### ReadSpanNonAlloc&lt;T&gt;(NetworkReader, Span&lt;T&gt;, Int32)



##### Declaration

```cs
public static int ReadSpanNonAlloc<T>(this NetworkReader reader, Span<T> outSpan, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| Span&lt;T&gt; | outSpan |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Int32 |  |

#### WriteCountPlusOne(NetworkWriter, Nullable&lt;Int32&gt;)

Writes null as 0, and all over values as +1


##### Declaration

```cs
public static void WriteCountPlusOne(NetworkWriter writer, int? count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.Nullable&lt;System.Int32&gt; | count | The real count or null if collection is is null |


#### ReadCountPlusOne(NetworkReader, out Int32)

Reads 0 as null, and all over values as -1


##### Declaration

```cs
public static bool ReadCountPlusOne(NetworkReader reader, out int count)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | count | The real count of the  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.Boolean | true if collection has value, false if collection is null |

#### ValidateSize(NetworkReader, Int32)


Use to check max size in reader before allocating array/list
Assumes each element is only 1 bit, so max size allocated will be MTU*8 if attacks tries to attack



##### Declaration

```cs
public static void ValidateSize(NetworkReader reader, int lengthInBits)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | lengthInBits |  |


