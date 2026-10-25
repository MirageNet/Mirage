---
id: StringStoreBrotliEncoderExtensions
title: StringStoreBrotliEncoderExtensions
---

# Class StringStoreBrotliEncoderExtensions



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
public static class StringStoreBrotliEncoderExtensions
```

### Methods
#### WriteStringStoreLengthsMessage(NetworkWriter, StringStoreLengthsMessage)



##### Declaration

```cs
public static void WriteStringStoreLengthsMessage(this NetworkWriter writer, StringStoreLengthsMessage part)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Serialization.BrotliCompression.StringStoreLengthsMessage | part |  |


#### ReadStringStoreLengthsMessage(NetworkReader)



##### Declaration

```cs
public static StringStoreLengthsMessage ReadStringStoreLengthsMessage(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.BrotliCompression.StringStoreLengthsMessage |  |

#### WriteStringStoreStringsMessage(NetworkWriter, StringStoreStringsMessage)



##### Declaration

```cs
public static void WriteStringStoreStringsMessage(this NetworkWriter writer, StringStoreStringsMessage part)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkWriter | writer |  |
| Mirage.Serialization.BrotliCompression.StringStoreStringsMessage | part |  |


#### ReadStringStoreStringsMessage(NetworkReader)



##### Declaration

```cs
public static StringStoreStringsMessage ReadStringStoreStringsMessage(this NetworkReader reader)
```
##### Parameters
| Type | Name | Description |
| ---- | ---- | ---- |
| Mirage.Serialization.NetworkReader | reader |  |

##### Returns
| Type | Description |
| ---- | ---- |
| Mirage.Serialization.BrotliCompression.StringStoreStringsMessage |  |

