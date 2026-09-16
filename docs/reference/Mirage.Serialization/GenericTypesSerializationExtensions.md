---
id: GenericTypesSerializationExtensions
title: GenericTypesSerializationExtensions
---

# Class GenericTypesSerializationExtensions



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
public static class GenericTypesSerializationExtensions
```

### Methods
#### Write&lt;T&gt;(NetworkWriter, T)


Writes any type that mirage supports



##### Declaration

```cs
public static void Write<T>(this NetworkWriter writer, T value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| T | value |  |


#### WriteWithLength&lt;T&gt;(NetworkWriter, T, Int32)


Writes any type that mirage supports with a maximum limit constraint



##### Declaration

```cs
public static void WriteWithLength<T>(this NetworkWriter writer, T value, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| T | value |  |
| System.Int32 | maxLength |  |


#### Read&lt;T&gt;(NetworkReader)


Reads any data type that mirage supports



##### Declaration

```cs
public static T Read<T>(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

#### ReadWithLength&lt;T&gt;(NetworkReader, Int32)


Reads any data type that mirage supports with a maximum limit constraint



##### Declaration

```cs
public static T ReadWithLength<T>(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| T |  |

