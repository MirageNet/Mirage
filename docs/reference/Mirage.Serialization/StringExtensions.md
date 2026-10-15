---
id: StringExtensions
title: StringExtensions
---

# Class StringExtensions



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
public static class StringExtensions
```


### Fields

#### defaultEncoding

##### Declaration

```cs
public static readonly UTF8Encoding defaultEncoding
```

### Properties

#### MaxStringLength

Maximum number of bytes a string can be serialized to. This is to avoid allocation attack.
Defaults MTU, 1300
NOTE: this is byte size after Encoding
IMPORTANT: Setting this property will resize the internal buffer. Do not call in hotpath. It is best to call once when you start the application


##### Declaration

```cs
public static int MaxStringLength { get; set; }
```
### Methods
#### WriteString(NetworkWriter, String)



##### Declaration

```cs
public static void WriteString(this NetworkWriter writer, string value)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.String | value | string or null |


#### ReadString(NetworkReader)



##### Declaration

```cs
public static string ReadString(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String | string or null |

#### WriteString(NetworkWriter, String, Encoding)



##### Declaration

```cs
public static void WriteString(this NetworkWriter writer, string value, Encoding encoding)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.String | value | string or null |
| System.Text.Encoding | encoding | Use this for encoding other than the default (UTF8). Make sure to use same encoding for ReadString |


#### ReadString(NetworkReader, Encoding)



##### Declaration

```cs
public static string ReadString(this NetworkReader reader, Encoding encoding)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Text.Encoding | encoding | Use this for encoding other than the default (UTF8). Make sure to use same encoding for WriterString |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String | string or null |

#### WriteString(NetworkWriter, String, Int32)



##### Declaration

```cs
public static void WriteString(this NetworkWriter writer, string value, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.String | value |  |
| System.Int32 | maxLength |  |


#### WriteString(NetworkWriter, String, Int32, Encoding)



##### Declaration

```cs
public static void WriteString(this NetworkWriter writer, string value, int maxLength, Encoding encoding)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| System.String | value |  |
| System.Int32 | maxLength |  |
| System.Text.Encoding | encoding |  |


#### ReadString(NetworkReader, Int32)



##### Declaration

```cs
public static string ReadString(this NetworkReader reader, int maxLength)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

#### ReadString(NetworkReader, Int32, Encoding)



##### Declaration

```cs
public static string ReadString(this NetworkReader reader, int maxLength, Encoding encoding)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |
| System.Int32 | maxLength |  |
| System.Text.Encoding | encoding |  |

##### Returns
| Type | Description |
| ---- | ---- |
| System.String |  |

